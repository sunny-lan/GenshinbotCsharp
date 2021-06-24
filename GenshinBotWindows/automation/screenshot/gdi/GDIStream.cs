using genshinbot.diag;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vanara.PInvoke;
using genshinbot.reactive;
using System.Reactive.Subjects;
using genshinbot.reactive.wire;
using genshinbot.util;

namespace genshinbot.automation.screenshot.gdi
{
    using Snap = Pkt<Mat>;

    class GDIStream : ScreenshotObservable
    {
        private Gdi32.SafeHDC hDesktopDC;
        private Gdi32.SafeHDC hTmpDC;
        private Gdi32.SafeHBITMAP sec;
        private Mat buf;

        ~GDIStream()
        {
            sec.Dispose();
            hTmpDC.Dispose();
            hDesktopDC.Dispose();
        }

        public GDIStream(ILiveWire<bool> enable = null)
        {
            hDesktopDC = User32.GetDC(IntPtr.Zero);
            if (hDesktopDC.IsInvalid)
            {
                throw new Exception("failed to get DC (to get pixel colors)");
            }

            hTmpDC = Gdi32.CreateCompatibleDC(hDesktopDC);

            //TODO support changing desktop sizes
            DPIAware.Use(DPIAware.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE, () =>
            {
                CreateBuffer(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            });

            this.enable = enable ?? new ConstLiveWire<bool>(true);

            poller = Wire.InfiniteLoop(Poll)
                .Relay(this.enable);
        }


        void CreateBuffer(int width, int height)
        {
            if (sec != null)
                sec.Dispose();
            buf = null;

            Gdi32.BITMAPINFO bi = new Gdi32.BITMAPINFO();
            bi.bmiHeader = new Gdi32.BITMAPINFOHEADER
            {
                biWidth = width,
                biHeight = -height,
                biPlanes = 1,
                biBitCount = 24,
                biCompression = Gdi32.BitmapCompressionMode.BI_RGB,
                biSize = Marshal.SizeOf(bi.bmiHeader)
            };
            IntPtr raw;
            sec = Gdi32.CreateDIBSection(hTmpDC, ref bi,
                Gdi32.DIBColorMode.DIB_RGB_COLORS,
                out raw, Gdi32.HSECTION.NULL, 0);
            if (sec.IsInvalid)
                Kernel32.GetLastError().ThrowIfFailed("failed creating dib section");

            buf = new Mat(height, width, OpenCvSharp.MatType.CV_8UC3, raw);
            hTmpDC.SelectObject(sec);   

        }

        void Poll()
        {
            Debug.Assert(buf != null);
            List<Rect> updates;
            DateTime snapTime;
            lock (pollRegions)
            {
                updates = new List<Rect>(pollRegions.Count);
                foreach (var region in pollRegions)
                {

                    //TODO this keeps failing!
                    //Console.WriteLine($"capture {region}");
                    if (!Gdi32.BitBlt(hTmpDC, region.X, region.Y, region.Width, region.Height, hDesktopDC,
                        region.X, region.Y, Gdi32.RasterOperationMode.SRCCOPY
                    ))
                    {
                        Debug.Assert(false, "Failed bitblt");
                        continue;
                    }
                    updates.Add(region);
                } 

                //Console.WriteLine("flush screenshot");
                if (!Gdi32.GdiFlush())
                {
                    throw new Exception("Failed gdiflush", Kernel32.GetLastError().GetException());
                }
                snapTime = DateTime.Now;
            }
            
            foreach (var update in updates)
            {
                //TODO bad!
                allSnaps.Emit((update, new Snap(buf, snapTime)));
            }
        }
        private ILiveWire<bool> enable;


        IWire<NoneT> poller;

        /// <summary>
        /// min number of distinct rects before applying merge algo
        /// </summary>
        public int MinRectsBeforeMerge = 0;

        /// <summary>
        /// cache of IWires for each screen region being watched
        /// </summary>
        Dictionary<Rect, IWire<Snap>> cache = new Dictionary<Rect, IWire<Snap>>();
        Dictionary<Rect,NoneT> listeningRects = new Dictionary<Rect, NoneT>();

        /// <summary>
        /// List of regions to actually poll
        /// </summary>
        List<Rect> pollRegions = new List<Rect>();

        /// <summary>
        /// contains events from all snapping
        /// </summary>
        WireSource<(Rect r, Snap s)> allSnaps=new WireSource<(Rect r, Snap s)>();

        /// <summary>
        /// Reevaluate what strategy is used to screenshot all the rects being watched
        /// </summary>
        void RecalculateStrategy()
        {
            lock (pollRegions)
            {
                pollRegions.Clear();


                // When we have too many rects
                // We will merge them into subrects
                if (listeningRects.Count > MinRectsBeforeMerge)
                {
                    Rect? bounds = null;
                    foreach (var x in listeningRects.Keys)
                        bounds = bounds?.Union(x) ?? x;
                    pollRegions.Add(bounds.Expect());
                }
                else
                {
                    // first we will eliminate rects which are nested
                    var orignal = new List<Rect>(listeningRects.Keys);
                    pollRegions.AddRange(orignal);
                }
            }
        }



        public IWire<Snap> Watch(Rect r)
        {
            if (!cache.ContainsKey(r))
            {
                cache[r] = allSnaps
                    .Link<(Rect r, Snap s),Snap>((snap, next)=> {
                        if (snap.r.IntersectsWith(r))
                        {
                            Snap res = snap.s.Select(x=>x[r]);
                            next(res);
                        }
                    })
                    .OnSubscribe(() =>
                    {
                        listeningRects[r] = NoneT.V;
                        Console.WriteLine($"gdi begin {r}");
                        RecalculateStrategy();
                        var pollerRef=poller.Use();//increment poller refcount
                        return DisposableUtil.From(() =>
                        {
                            pollerRef.Dispose();
                            Console.WriteLine($"gdi stop {r}");
                            Debug.Assert(listeningRects.Remove(r, out var _));
                            RecalculateStrategy();
                        });
                    });

            }
            return cache[r];
        }
        public static async Task Test2()
        {
            //Task.Run(()=> { while (true) Cv2.WaitKey(1); });\
            var enable = new LiveWireSource<bool>(true);
            GDIStream strm = new GDIStream(enable);
            var poll = strm.Watch(new Rect(0, 0, 1600, 900));//.Debug("screenshot 1600x900");
            CvThread.ImShow("a", await poll.Depacket().Get());
            Console.WriteLine("b:");
            Console.ReadLine();//ds
            int fps = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (poll.Subscribe(m =>
            {
                CvThread.ImShow("a", m.Value);
                //    Console.WriteLine("frame");
                fps++;
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    Console.WriteLine("Fps: " + fps);
                    fps = 0;
                    sw.Restart();
                }
            }))
            using (strm.Watch(new Rect(300, 100, 100, 100)).Subscribe(m =>
            {
                CvThread.ImShow("b", m.Value);
            }))
            using (strm.Watch(new Rect(300, 300, 100, 100)).Subscribe(m =>
            {
                CvThread.ImShow("c", m.Value);
            }))
            {
                Console.WriteLine("a:");
                bool v = true;
                while (true)
                {
                    Console.ReadLine();
                    v = !v;
                    Console.WriteLine($"enable={v}");
                    enable.SetValue(v);
                }
            }

        }
        /*
                public static void Test()
                {
                    IWire<Unit> tmp = Observable.FromEvent(
                        h => Console.WriteLine("add"),
                        h => Console.WriteLine("remove")
                    );

                    Console.WriteLine("a");
                    Console.ReadKey();
                    Console.WriteLine("b");
                    using (var o = tmp.Subscribe(Observer.Create<Unit>(_ => Console.WriteLine("evt"))))
                    {
                        Console.ReadKey();
                        Console.WriteLine("d");
                        using (var b = tmp.Subscribe(Observer.Create<Unit>(_ => Console.WriteLine("evt"))))
                        {
                            Console.ReadKey();
                        }
                        Console.WriteLine("e");
                        Console.ReadKey();
                    }
                    Console.WriteLine("c");

                }*/
    }
}

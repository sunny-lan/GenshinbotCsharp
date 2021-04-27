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

namespace genshinbot.automation.screenshot.gdi
{

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

        public GDIStream()
        {
            hDesktopDC = User32.GetDC(IntPtr.Zero);
            if (hDesktopDC.IsInvalid)
            {
                throw new Exception("failed to get DC (to get pixel colors)");
            }

            hTmpDC = Gdi32.CreateCompatibleDC(hDesktopDC);

            //TODO support changing desktop sizes
            CreateBuffer(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);

            poller = new reactive.Poller<Mat>(Poll);
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
                biBitCount = 32,
                biCompression = Gdi32.BitmapCompressionMode.BI_RGB,
                biSize = Marshal.SizeOf(bi.bmiHeader)
            };
            IntPtr raw;
            sec = Gdi32.CreateDIBSection(hTmpDC, ref bi,
                Gdi32.DIBColorMode.DIB_RGB_COLORS,
                out raw, Gdi32.HSECTION.NULL, 0);
            if (sec.IsInvalid)
                Kernel32.GetLastError().ThrowIfFailed("failed creating dib section");

            buf = new Mat(height, width, OpenCvSharp.MatType.CV_8UC4, raw);
            hTmpDC.SelectObject(sec);
        }

        Mat Poll()
        {
            Debug.Assert(buf != null);
            if (listeningRects.Count > MinRectsBeforeMerge)
            {
                if (bounds is Rect region)
                    if (!Gdi32.BitBlt(hTmpDC, region.X, region.Y, region.Width, region.Height, hDesktopDC,
                        region.X, region.Y, Gdi32.RasterOperationMode.SRCCOPY
                    ))
                        Kernel32.GetLastError().ThrowIfFailed("failed performing BitBlt");
            }
            else
            {
                //  Console.WriteLine("refresh");
                foreach (var region in listeningRects)
                {
                    if (!Gdi32.BitBlt(hTmpDC, region.X, region.Y, region.Width, region.Height, hDesktopDC,
                        region.X, region.Y, Gdi32.RasterOperationMode.SRCCOPY
                    ))
                        Kernel32.GetLastError().ThrowIfFailed("failed performing BitBlt");
                }
            }

            if (!Gdi32.GdiFlush())
                Kernel32.GetLastError().ThrowIfFailed("failed performing GdiFlush");

            return buf;
        }
        reactive.Poller<Mat> poller;

        /// <summary>
        /// min number of distinct rects before switching modes
        /// </summary>
        public int MinRectsBeforeMerge = 2;

        /// <summary>
        /// cache of IObservables for each screen region being watched
        /// </summary>
        Dictionary<Rect, IObservable<Mat>> cache = new Dictionary<Rect, IObservable<Mat>>();


        /// <summary>
        /// list of rects which are currently being watched
        /// </summary>
        ICollection<Rect> listeningRects => thing.Keys;
        ConcurrentDictionary<Rect, Unit> thing = new ConcurrentDictionary<Rect, Unit>();

        /// <summary>
        /// the overall bounding rect of all the rects in listeningRects
        /// </summary>
        Rect? bounds;

        /// <summary>
        /// Reevaluate what strategy is used to screenshot all the rects being watched
        /// </summary>
        void RecalculateStrategy()
        {
            foreach (var x in listeningRects)
                bounds = bounds?.Union(x) ?? x;
        }

        public IObservable<Mat> Watch(Rect r)
        {
            if (!cache.ContainsKey(r))
            {
                //a dummy observable to update the list of rects
                var boundsCalcer = Observable.FromEvent<Mat>(h =>
                {
                    thing[r] = default;
                    RecalculateStrategy();
                }, h =>
                {
                    Debug.Assert(thing.Remove(r, out var _));
                    RecalculateStrategy();
                });
                cache[r] = Observable.Merge(boundsCalcer, poller.Select(m => m[r]));

            }
            return cache[r];
        }
        public static void Test2()
        {
            //Task.Run(()=> { while (true) Cv2.WaitKey(1); });
            GDIStream strm = new GDIStream();
            var poll = strm.Watch(new Rect(0, 0, 1600, 900));
            Console.WriteLine("b:");
            Console.ReadLine();//ds
            int fps = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (poll.Subscribe(m =>
            {
                CvThread.ImShow("a", m);
                //    Console.WriteLine("frame");
                fps++;
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    Console.WriteLine("Fps: " + fps);
                    fps = 0;
                    sw.Restart();
                }
            }))
            using (strm.Watch(new Rect(300, 100, 100, 100)).Subscribe(Observer.Create<Mat>(m =>
            {
                CvThread.ImShow("b", m);
            })))
            using (strm.Watch(new Rect(300, 300, 100, 100)).Subscribe(Observer.Create<Mat>(m =>
            {
                CvThread.ImShow("c", m);
            })))
            {
                Console.WriteLine("a:");
                Console.ReadLine();
            }

        }

        public static void Test()
        {
            IObservable<Unit> tmp = Observable.FromEvent(
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

        }
    }
}

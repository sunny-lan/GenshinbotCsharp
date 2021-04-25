using OpenCvSharp;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation.screenshot.directx
{
    class Test
    {
        public static void Run()
        {
            using var factory = new Factory1();
            foreach (var adapter in factory.Adapters1)
            {
                Console.WriteLine(adapter.Description.Description);
                Console.WriteLine("-----");
            }
            using var adapter1 = factory.Adapters[0];
            foreach (var output in adapter1.Outputs)
            {
                Console.WriteLine(output.Description.DeviceName);
                Console.WriteLine("-----");
            }
            using var output1 = adapter1.Outputs[0].QueryInterface<Output1>();

            var flags = DeviceCreationFlags.BgraSupport; // for D2D cooperation
            flags |= DeviceCreationFlags.VideoSupport;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif

            using var device = new SharpDX.Direct3D11.Device(adapter1, flags);
            using (var mt = device.QueryInterface<DeviceMultithread>())
            {
                mt.SetMultithreadProtected(new RawBool(true));
            }
            using var duplication = output1.DuplicateOutput(device);
            var desc = duplication.Description.ModeDescription;

            Console.WriteLine("w:{0} h:{1}", desc.Width, desc.Height);

            //Cv2.NamedWindow("a", WindowFlags.FreeRatio);
            Texture2DDescription description = new Texture2DDescription
            {
                Width = desc.Width,
                Height = desc.Height,
                Format = desc.Format,
                ArraySize = 1,
                BindFlags = BindFlags.None,
                SampleDescription = new SampleDescription { Count = 1, Quality = 0, },

                MipLevels = 1,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging,

            };
         //   Rect sub = new Rect(51, 15, 178, 178);
            Rect sub = new Rect(00, 0, 1600, 900);
          //  description.Width = sub.Width;
          //  description.Height = sub.Height;

            SharpDX.DXGI.Resource frame;
            OutputDuplicateFrameInformation frameInfo;
            using var dstTex = new Texture2D(device, description);
            /*  
                Rect sub2 = new Rect(510, 150, 178, 178);
                description.Width = sub2.Width;
                description.Height = sub2.Height;
            using var dstTex2 = new Texture2D(device, description);*/
            using var ctx = device.ImmediateContext;
            Stopwatch timer = new Stopwatch();
            int fps = 0;
            int fps_store = 0;
            timer.Start();
            var mapped = ctx.MapSubresource(dstTex, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
            Mat img = new Mat(dstTex.Description.Height, dstTex.Description.Width, MatType.CV_8UC4, mapped.DataPointer, mapped.RowPitch);
            ctx.UnmapSubresource(dstTex, 0);

          /*  var mapped2 = ctx.MapSubresource(dstTex2, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
            Mat img3 = new Mat(dstTex2.Description.Height, dstTex.Description.Width, MatType.CV_8UC4, mapped2.DataPointer, mapped.RowPitch);
            ctx.UnmapSubresource(dstTex2, 0);*/
            bool hasPrev = false;
            do
            {
                if (hasPrev)
                    duplication.ReleaseFrame();
                var result = duplication.TryAcquireNextFrame(10000, out frameInfo, out frame);
                if (result.Code != SharpDX.DXGI.ResultCode.AccessLost.Code)
                {
                    result.CheckError();
                    if (frameInfo.LastPresentTime != 0)
                    {



                        using var srcTex = frame.QueryInterface<Texture2D>();
                       // ctx.CopyResource(srcTex, dstTex);
                       ctx.CopySubresourceRegion(srcTex, 0,
                            new ResourceRegion(left: sub.Left, right: sub.Right, top: sub.Top, bottom: sub.Bottom,
                            front: 0, back: 1),
                           dstTex, 0
                           //, dstX: sub.X, dstY: sub.Y
                            ); 
                       /* ctx.CopySubresourceRegion(srcTex, 0,
                             new ResourceRegion(left: sub2.Left, right: sub2.Right, top: sub2.Top, bottom: sub2.Bottom,
                             front: 0, back: 1),
                            dstTex2, 0
                             //, dstX: sub.X, dstY: sub.Y
                             );*/
                        //   img = img1[sub];
                        img.PutText(fps_store.ToString(), new Point(20, 20), HersheyFonts.HersheyPlain, fontScale: 1, color: Scalar.Red, thickness: 2);
                            Cv2.ImShow("a", img); 
                        //Cv2.ImShow("b", img3);
                    }
                    Cv2.WaitKey(1);
                    frame.Dispose();
                    hasPrev = true;
                    fps++;
                    if (timer.ElapsedMilliseconds >= 1000)
                    {
                        fps_store = fps;
                        fps = 0;

                        timer.Restart();
                    }
                }
            } while (true);

        }
    }
}

using OpenCvSharp;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
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
            SharpDX.DXGI.Resource frame;
            OutputDuplicateFrameInformation frameInfo;
            using var dstTex = new Texture2D(device, description);
            using var ctx = device.ImmediateContext;

            do
            {
                var result = duplication.TryAcquireNextFrame(10000, out frameInfo, out frame);
                if (result.Code != SharpDX.DXGI.ResultCode.AccessLost.Code)
                {
                    result.CheckError();
                    if (frameInfo.LastPresentTime != 0)
                    {



                        using var srcTex = frame.QueryInterface<Texture2D>();
                        ctx.CopyResource(srcTex, dstTex);
                        // var sub= SharpDX.Direct3D11.Resource.CalculateSubResourceIndex(0, 0, 0);
                        var mapped = ctx.MapSubresource(dstTex, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);


                        using var img = new Mat(desc.Height, desc.Width, MatType.CV_8UC4, mapped.DataPointer, mapped.RowPitch);

                        Cv2.ImShow("a", img);
                        Cv2.WaitKey(1);
                        ctx.UnmapSubresource(dstTex, 0);

                    }
                    frame.Dispose();

                    duplication.ReleaseFrame();
                }
            } while (true);

        }
    }
}

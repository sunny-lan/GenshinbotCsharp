using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace genshinbot.automation.screenshot
{
    class gdi
    {
        public static void run()
        {
            var r = SHCore.SetProcessDpiAwareness(SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            if (r != HRESULT.S_OK)
                throw r.GetException("failed to set dpi awareness to per monitor aware. this is required for multimonitor screenshots");

            var hDesktopDC = User32.GetDC(IntPtr.Zero);
            if (hDesktopDC.IsInvalid)
                throw new Exception("failed to get DC (to get pixel colors)");

            var hTmpDC = Gdi32.CreateCompatibleDC(hDesktopDC);
            Rect sub = new Rect(00, 0, 1600, 900);
            int width = sub.Width, height = sub.Height;
            Gdi32.BITMAPINFO bi = new Gdi32.BITMAPINFO();
            bi.bmiHeader = new Gdi32.BITMAPINFOHEADER
            {
                biWidth = sub.Width,
                biHeight = -sub.Height,
                biPlanes = 1,
                biBitCount = 32,
                biCompression = Gdi32.BitmapCompressionMode.BI_RGB,
                biSize = Marshal.SizeOf(bi.bmiHeader)
            };
            IntPtr raw;
            var sec = Gdi32.CreateDIBSection(hTmpDC, ref bi,
                Gdi32.DIBColorMode.DIB_RGB_COLORS,
                out raw, Gdi32.HSECTION.NULL, 0);
            if (sec.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "failed to create dib section");

            var Mat = new OpenCvSharp.Mat(height, width, OpenCvSharp.MatType.CV_8UC4, raw);
            hTmpDC.SelectObject(sec);
            //
            //Rect sub = new Rect(51, 15, 178, 178);
            Rect[] regions = {
                //new Rect(100,100,300,100) ,
               /* new Rect(100,100,100,100),
                new Rect(200,100,100,100),
               new Rect(300,100,100,100)*/
               new Rect(0,0,1600,900)
            };

            Stopwatch timer = new Stopwatch();
            int fps = 0;
            int fps_store = 0;
            timer.Start();
            while (true)
            {
                fps++;
                if (timer.ElapsedMilliseconds >= 1000)
                {
                    fps_store = fps;
                    fps = 0;
                    Mat.SetTo(new Scalar(0, 0, 0, 0));
                    timer.Restart();
                }
                foreach (var region in regions)
                {
                    if (!Gdi32.BitBlt(hTmpDC, region.X, region.Y, region.Width, region.Height, hDesktopDC,
                        region.X, region.Y, Gdi32.RasterOperationMode.SRCCOPY
                    ) )
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if(!Gdi32.GdiFlush())
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                Mat.PutText(fps_store.ToString(), new Point(20, 20), HersheyFonts.HersheyPlain, fontScale: 1, color: Scalar.Red, thickness: 2);

                Cv2.ImShow("a", Mat);
                Cv2.WaitKey(1);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace GenshinbotCsharp
{
    class Screenshot
    {
        static Screenshot()
        {
            var r = SHCore.SetProcessDpiAwareness(SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            if (r != HRESULT.S_OK)
                throw r.GetException("failed to set dpi awareness to per monitor aware. this is required for multimonitor screenshots");

            hDesktopDC = User32.GetDC(IntPtr.Zero);
            if (hDesktopDC.IsInvalid)
                throw new Exception("failed to get DC (to get pixel colors)");

            hTmpDC = Gdi32.CreateCompatibleDC(hDesktopDC);
        }

        private static Gdi32.SafeHDC hDesktopDC;
        private static Gdi32.SafeHDC hTmpDC;
        public class Buffer : IDisposable
        {
            public IntPtr Raw;
            public Gdi32.SafeHBITMAP HBitmap;
            public OpenCvSharp.Mat Mat;

            public void Dispose() => HBitmap.Dispose();
        }

        /// <summary>
        /// Creates a opencv mat which is backed by GDI memory
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Buffer GetBuffer(int width, int height)
        {
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
            var sec = Gdi32.CreateDIBSection(hTmpDC, ref bi,
                Gdi32.DIBColorMode.DIB_RGB_COLORS,
                out raw, Gdi32.HSECTION.NULL, 0);
            if (sec.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "failed to create dib section");
            return new Buffer
            {
                HBitmap = sec,
                Raw = raw,
                Mat = new OpenCvSharp.Mat(height, width, OpenCvSharp.MatType.CV_8UC4, raw),
            };
        }

        /// <summary>
        /// screenshots rectangle starting at (x,y) in physical coordinates, into the whole img
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="img"></param>
        public static void Take(int x, int y, Buffer img)
        {
           

            hTmpDC.SelectObject(img.HBitmap);
            if (!Gdi32.BitBlt(hTmpDC, 0, 0, img.Mat.Width, img.Mat.Height, hDesktopDC,
               x, y, Gdi32.RasterOperationMode.SRCCOPY
                ) || !Gdi32.GdiFlush())
                throw new Win32Exception(Marshal.GetLastWin32Error());

        }

        public static Color GetPixelColor(int x, int y)
        {
            COLORREF pixel = Gdi32.GetPixel(hDesktopDC, x, y);
            Color color = Color.FromArgb(pixel.R, pixel.G, pixel.B);
            return color;
        }
    }
}

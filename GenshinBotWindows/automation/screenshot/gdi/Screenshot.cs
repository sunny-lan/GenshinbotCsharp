
using System;
using System.Collections.Generic;

namespace genshinbot
{
    using Vanara.PInvoke;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using OpenCvSharp;
    using System.Diagnostics;



    class Screenshot
    {

        static bool inited = false;
        public static void Init()
        {
            if (inited) return;
            inited = true;
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
        public class Buffer
        {
            public IntPtr Raw;
            public Gdi32.SafeHBITMAP HBitmap;
            public OpenCvSharp.Mat Mat;
            public Size Size=>Mat.Size();

            ~Buffer()
            {
                HBitmap.Dispose();
                Mat.Dispose();
            }
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
                Mat = new OpenCvSharp.Mat(height, width, OpenCvSharp.MatType.CV_8UC4, raw)
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

       /// <summary>
       /// Copies desktop rect(src,sz) to img rect(dst,sz)
       /// </summary>
        public static void Take( Buffer img, Size sz, Point src, Point dst)
        {
            Debug.Assert(dst.X >= 0 && dst.Y >= 0);
            Debug.Assert(dst.X+sz.Width <=img.Mat.Width && dst.Y+sz.Height <=img.Mat.Height);


            hTmpDC.SelectObject(img.HBitmap);
            lock(hDesktopDC)
            if (!Gdi32.BitBlt(hTmpDC, dst.X, dst.Y,sz.Width, sz.Height, hDesktopDC,
               src.X, src.Y, Gdi32.RasterOperationMode.SRCCOPY
                ) || !Gdi32.GdiFlush())
                throw new Win32Exception(Marshal.GetLastWin32Error());

        }

        public static Scalar GetPixelColor(int x, int y)
        {
            COLORREF pixel = Gdi32.GetPixel(hDesktopDC, x, y);
            return Scalar.FromRgb(pixel.R, pixel.G, pixel.B);
        }
    }
}

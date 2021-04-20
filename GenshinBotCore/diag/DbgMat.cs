using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot.diag
{
    /// <summary>
    /// Provides a way for other classes to display debug images but allow debug to be disabled when unneeded
    /// </summary>
    public class DbgMat
    {
        Mat img = new Mat();
        public string DbgWindowName;

        static int ctr = 0;
        static string genName()
        {
            return "dbg" + ctr++;
        }

        public DbgMat()
        {
            DbgWindowName = genName();
            OnDebugImg = DefaultAction;
        }

        ~DbgMat()
        {
            img.Dispose();
        }

        void DefaultAction(Mat m, bool w)
        {
            Cv2.ImShow(DbgWindowName, m);
            Cv2.WaitKey(w ? 0 : 1);
        }

        public Action<Mat, bool> OnDebugImg;
        public bool Enabled;

        public void Image(Mat a, Point? dst=null)
        {
            if (!Enabled) return;
            var d = dst ?? Util.Origin;
            img.Create(a.Rows + d.X, a.Cols + d.Y, a.Type());
            a.CopyTo(img[new Rect(d, a.Size())]);
        }

        public void Circle(Point center, int radius, Scalar color, int thickness = 1)
        {
            if (!Enabled) return;
            img.Circle(center, radius, color, thickness);
        }

        public void Rectangle(Rect r, Scalar color, int thickness = 1)
        {
            if (!Enabled) return;
            img.Rectangle(rect: r, color, thickness);
        }

        public void Line(Point a, Point b, Scalar color, int thickness = 1)
        {
            if (!Enabled) return;
            img.Line(a, b, color, thickness);
        }

        /// <summary>
        /// Call this function whenever the content in this debug mat is ready for display
        /// </summary>
        /// <param name="wait">Set to true in order to wait for user input before continuing</param>
        public void Flush(bool wait = false)
        {
            if (!Enabled) return;
            OnDebugImg?.Invoke(img, wait);
        }
    }
}

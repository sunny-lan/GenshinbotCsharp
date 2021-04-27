using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace genshinbot.diag
{
    /// <summary>
    /// Provides a way for other classes to display debug images but allow debug to be disabled when unneeded
    /// </summary>
    public class DbgMat : IObservable<Mat>
    {
        private Mat img= new Mat();
        public string DbgWindowName;
        private IObservable<Mat> thing;
        static int ctr = 0;
        static string genName()
        {
            return "dbg" + ctr++;
        }
        public IDisposable Show()
        {
            return this.Subscribe(x => Cv2.ImShow(DbgWindowName, x));
        }
        public DbgMat()
        {
            DbgWindowName = genName();
            thing = Observable.FromEvent<Mat>(x => OnDebugImg += x, x => OnDebugImg -= x).Replay(1);
        }

        ~DbgMat()
        {
            img.Dispose();
        }

        private event Action<Mat> OnDebugImg;
        private bool Enabled => OnDebugImg != null;

        public void Image(Mat a, Point? dst = null)
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
        public void Flush()
        {
            if (!Enabled) return;
            OnDebugImg?.Invoke(img);
        }

        public IDisposable Subscribe(IObserver<Mat> observer)
        {
            return thing.Subscribe(observer);
        }
    }
}

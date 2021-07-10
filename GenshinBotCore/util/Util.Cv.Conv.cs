
using OpenCvSharp;
using System;

namespace genshinbot
{
    public static partial class Util
    {
        public static System.Drawing.Color SysBgr255(this Scalar s)
        {
            return System.Drawing.Color.FromArgb(blue: (int)s.Val0, green: (int)s.Val1, red: (int)s.Val2);
        }
        public static Scalar cv3(this System.Drawing.Color c)
        {
            return Scalar.FromRgb(c.R, c.G, c.B);
        }
        public static Scalar Cvt(this Vec3f v)
        {
            return new Scalar(v.Item0, v.Item1, v.Item2);
        }
        public static Scalar Cvt(this Vec3b v)
        {
            return new Scalar(v.Item0, v.Item1, v.Item2);
        }

        public static Point Cv(this System.Drawing.Point p)
        {
            return new Point(p.X, p.Y);
        }
        public static System.Drawing.Point Sys(this Point p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }
        public static System.Drawing.Size Sys(this Size p)
        {
            return new System.Drawing.Size(p.Width, p.Height);
        }
        public static System.Drawing.Rectangle Sys(this Rect p)
        {
            return new System.Drawing.Rectangle(p.TopLeft.Sys(), p.Size.Sys());
        }

        public static System.Drawing.Bitmap ToBmpFast(this Mat m)
        {
            if (m.Channels() == 3)
                return new System.Drawing.Bitmap(m.Width, m.Height,
                    (int)m.Step(),
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                    m.Data);
            if (m.Channels() == 4)
                return new System.Drawing.Bitmap(m.Width, m.Height,
                    (int)m.Step(),
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                    m.Data);
            throw new Exception("unable to convert");
        }

        public static Point2d Cvt(this Point2f p)
        {
            return new Point2d(p.X, p.Y);
        }
        public static Point2d Cvt(this Point p)
        {
            return new Point2d(p.X, p.Y);
        }

        public static Point2d ToPointd(this Mat mat)
        {
            return new Point2d((float)mat.Get<double>(0, 0), (float)mat.Get<double>(1, 0));
        }
        public static Mat ToMat(this Point2d point)
        {
            return new Mat(3, 1, MatType.CV_64FC1, new double[] { point.X, point.Y, 1 });
        }

        public static Point2f ToPointf(this Point2d p)
        {
            return new Point2f((float)p.X, (float)p.Y);
        }

        public static Rect ImgRect(this Mat m)
        {
            return new Rect(Origin, m.Size());
        }

        public static Rect Cv(this System.Drawing.Rectangle p)
        {
            return new Rect(p.Location.Cv(), p.Size.cv());
        }

    }
}

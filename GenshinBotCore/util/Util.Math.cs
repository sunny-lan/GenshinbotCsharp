    
using OpenCvSharp;
using System;

namespace genshinbot
{
    public static partial class Util
    {

        public static int Area(this Rect r)
        {
            return r.Width * r.Height;
        }
        /// <summary>
        /// converts value in range 0-1 to range in1-in2
        /// </summary>
        /// <param name="v"></param>
        /// <param name="in1"></param>
        /// <param name="in2"></param>
        /// <returns></returns>
        public static double Denormalize(this double v, double in1, double in2)
        {
            return v * (in2 - in1) + in1;
        }

        /// <summary>
        ///  converts value in range  in1-in2 to range 0-1
        /// </summary>
        /// <param name="v"></param>
        /// <param name="in1"></param>
        /// <param name="in2"></param>
        /// <returns></returns>
        public static double Normalize(this double v, double in1, double in2)
        {
            return (v - in1) / (in2 - in1);
        }
        public static double Map(this double v, double in1, double in2, double out1, double out2)
        {
            return v.Normalize(in1, in2).Denormalize(out1, out2);
        }

        public static bool Contains(this Rect r, Point2d p)
        {
            return r.cvt().Contains(p);
        }

        public static double ConfineAngle(this double a)
        {
            a %= Math.PI * 2;
            if (a < 0) a += Math.PI * 2;
            return a;
        }
        public static double SmallerAngleBetween(double a, double b)
        {
            var diff = Math.Abs(a.ConfineAngle() - b.ConfineAngle());
            return Math.Min(diff, 2 * Math.PI - diff);
        }
        public static System.Drawing.Rectangle Union(this System.Drawing.Rectangle a, System.Drawing.Rectangle b)
        {
            var r = new System.Drawing.Rectangle
            {
                X = Math.Min(a.X, b.X),
                Y = Math.Min(a.Y, b.Y),
            };
            r.Width = Math.Max(a.Right, b.Right) - r.Left;
            r.Height = Math.Max(a.Bottom, b.Bottom) - r.Top;
            return r;
        }

        public static double RelativeAngle(this double a, double b)
        {
            var diff = (b.ConfineAngle() - a.ConfineAngle()).ConfineAngle();
            if (diff > Math.PI) return -(Math.PI * 2 - diff);
            else return diff;
        }
        public static double Angle(this Point2d p)
        {
            return Math.Atan2(p.Y, p.X);
        }
        public static double Length(this Point2d p)
        {
            return p.DistanceTo(Origin);
        }
        public static long Area(this Size sz)
        {
            return sz.Width * sz.Height;
        }

        public static double Radians(this double deg)
        {
            return deg * Math.PI / 180;
        }


        public static void fftShift(this Mat src, Mat dst)
        {
            int cx = src.Width >> 1;
            int cy = src.Height >> 1;
            dst.Create(src.Size(), src.Type());
            src[new Rect(0, 0, cx, cy)].CopyTo(dst[new Rect(cx, cy, cx, cy)]);
            src[new Rect(cx, cy, cx, cy)].CopyTo(dst[new Rect(0, 0, cx, cy)]);
            src[new Rect(cx, 0, cx, cy)].CopyTo(dst[new Rect(0, cy, cx, cy)]);
            src[new Rect(0, cy, cx, cy)].CopyTo(dst[new Rect(cx, 0, cx, cy)]);
        }
        public static double scoreFunc(double a, double b)
        {
            return 1.0 / (1 + Math.Abs(a - b));
        }
        public static double Degrees(this double rad)
        {
            return rad * 180 / Math.PI;
        }
        public static Point Round(this Point2d p)
        {
            return new Point(p.X, p.Y);
        }
        public static double AngleTo(this Point2d a, Point2d b)
        {
            var diff = b - a;
            return Math.Atan2(diff.Y, diff.X);
        }

        public static Point2d Norm(this Point2d p)
        {

            if (p == Origin)
                return Origin;
            return p * (1.0 / p.Length());
        }

        public static Point2d LimitDistance(this Point2d p, double maxDist)
        {
            return p.Norm() * Math.Min(p.DistanceTo(Origin), maxDist);
        }
        public static Point2d LimitDistance(this Point2d p, double maxDist, out double outputlength)
        {
            outputlength = Math.Min(p.DistanceTo(Origin), maxDist);
            return p.Norm() * outputlength;
        }

        public static Point2d Vec(this double angle, double mag = 1)
        {
            return new Point2d(x: Math.Cos(angle) * mag, y: Math.Sin(angle) * mag);
        }
    }
}

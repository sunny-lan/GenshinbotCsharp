
using OpenCvSharp;
using System;

namespace genshinbot
{
    public static partial class Util { 
    
        public static Point2d Closest(this Point2d p, Point2d[] pp)
        {
            Point2d r = default;
            double mx = double.PositiveInfinity;
            foreach (var x in pp)
                if (x.DistanceTo(p) < mx)
                    r = x;
            return r;
        }

        public static Point2d Center(this Rect img)
        {
            return new Point2d((img.Left + img.Right) / 2.0, (img.Top + img.Bottom) / 2.0);
        }
        public static Point2d Center(this Mat img)
        {
            return new Point2d(img.Width / 2.0, img.Height / 2.0);
        }
        public static Point2d Center(this Size img)
        {
            return new Point2d(img.Width / 2.0, img.Height / 2.0);
        }

        public static Size Scale(this Size size, double s)
        {
            return new Size(size.Width * s, size.Height * s);
        }
        public static Size round(this Size2d size)
        {
            return new Size(size.Width, size.Height);
        }
        public static Size2d cvt(this Size size)
        {
            return new Size2d(size.Width, size.Height);
        }
        public static Size2d Scale(this Size2d size, double s)
        {
            return new Size2d(size.Width * s, size.Height * s);
        }
        public static  Rect2d cvt(this Rect a)
        {
            return new Rect2d(a.X,a.Y,a.Width,a.Height);
        }
        public static Rect round(this Rect2d a)
        {
            return new Rect(a.Location.Round(),a.Size.round());
        }
        public static Size cv(this System.Drawing.Size s)
        {
            return new Size(s.Width, s.Height);
        }

        public static Point2d[] Corners(this Rect2d r)
        {
            return new Point2d[] { r.TopLeft, r.BottomRight, new Point2d(r.Right, r.Top), new Point2d(r.Left, r.Bottom) };
        }

        public static Point ReadPoint()
        {
            Console.Write("x:");
            int x = int.Parse(Console.ReadLine());
            Console.Write("y:");
            int y = int.Parse(Console.ReadLine());
            return new Point(x, y);
        }

        private static Mat conv_output = new Mat();
        private static Mat conv_input = new Mat(new Size(1, 1), MatType.CV_8UC3);
        public static Scalar CvtColor(this Scalar s, ColorConversionCodes code)
        {
            lock (conv_input)
            {
                conv_input.SetTo(s);
                Cv2.CvtColor(conv_input, conv_output, code);
                var thing = conv_output.Get<Vec3b>(0);
                return thing.Cvt();
            }
        }

        public static double SumComponents(this Scalar s)
        {
            return s.Val0 + s.Val1 + s.Val2 + s.Val3;
        }
        public static Rect Bounds(this Size sz, Point? topleft = null)
        {
            return new Rect(topleft ?? Util.Origin, sz);
        }
        public static Rect RectAround(Point initial, Point final)
        {
            var r = new OpenCvSharp.Rect();
            r.Left = Math.Min(initial.X, final.X);
            r.Width = Math.Max(initial.X, final.X) - r.Left;
            r.Top = Math.Min(initial.Y, final.Y);
            r.Height = Math.Max(initial.Y, final.Y) - r.Top;
            return r;
        }
        public static Rect2d RectAround(Point2d initial, Point2d final)
        {
            var r = new OpenCvSharp.Rect2d();
            r.Left = Math.Min(initial.X, final.X);
            r.Width = Math.Max(initial.X, final.X) - r.Left;
            r.Top = Math.Min(initial.Y, final.Y);
            r.Height = Math.Max(initial.Y, final.Y) - r.Top;
            return r;
        }
        public static ConnectedComponents.Blob FindBiggestBlob(Mat src)
        {
            var comps = Cv2.ConnectedComponentsEx(src, PixelConnectivity.Connectivity4);
            ConnectedComponents.Blob res = default;
            int maxArea = -1;
            foreach (var blob in comps.Blobs)
            {
                if (blob.Label == 0) continue;
                if (blob.Area > maxArea)
                {
                    maxArea = blob.Area;
                    res = blob;
                }
            }
            return res;
        }
        public static Point RandomWithin(this Rect r)
        {
            return new Point(rng.Next(r.Left, r.Right), rng.Next(r.Top, r.Bottom));
        }
        public static Point Origin = new Point(0, 0);

        public static Rect RectAround(this Point2d p, Size sz)
        {
            return new Rect((p - sz.Center()).Round(), sz);
        }
        public static Rect RectAround(this Point2d p, int sz)
        {
            return p.RectAround(new Size(sz, sz));
        }

        public static Rect Pad(this Rect r, int sz)
        {
            return new Rect(r.X - sz, r.Y - sz, r.Width + sz * 2, r.Height + sz * 2);
        }
        public static Size Pad(this Size r, int sz)
        {
            return new Size(r.Width + sz * 2, r.Height + sz * 2);
        }

    }
}

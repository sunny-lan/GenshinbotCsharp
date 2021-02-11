using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    static class Util
    {
        private static Random rng = new Random(1);

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static double scoreFunc(double a, double b)
        {
            return 1.0 / (1 + Math.Abs(a - b));
        }
        public static Point ToPoint(this Point2d p)
        {
            return new Point(p.X, p.Y);
        }
        public static double AngleTo(this Point2d a,  Point2d b)
        {
            var diff = b - a;
            return Math.Atan2(diff.Y, diff.X);
        }
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        public static List<T> ToList<T>(this IEnumerator<T> enumerator)
        {
            return enumerator.ToEnumerable().ToList();
        }

        public static int LowerBound<T>(this List<T> list, Func<T,bool> pred)
        {
            int lo = 0, hi = list.Count;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (pred(list[mid]))
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;
                }
            }
            return lo;
        }

        public static Rect ToOpenCVRect(this Vanara.PInvoke.RECT r)
        {
            return new Rect(r.X, r.Y, r.Width, r.Height);
        }

        public static Point2d ToPointd(this Mat mat)
        {
            return new Point2d((float)mat.Get<double>(0, 0), (float)mat.Get<double>(1   , 0));
        }
        public static Mat ToMat(this Point2d point)
        {
            return new Mat(3, 1, MatType.CV_64FC1, new double[] { point.X, point.Y, 1 });
        }

        public static Point2f ToPointf(this Point2d p)
        {
            return new Point2f((float)p.X, (float)p.Y);
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
    }
}

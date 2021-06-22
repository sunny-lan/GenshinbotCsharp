using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot
{
    public static class Util
    {
        public static long Area(this Size sz)
        {
            return sz.Width * sz.Height;
        }
        public static Rect Bounds(this Size sz, Point? topleft=null)
        {
            return new Rect(topleft ?? Util.Origin, sz);
        }
        /// <summary>
        /// Returns an observable which publishes unit.default everytime a new element is recieved.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static IObservable<Unit> ToUnit<T>(this IObservable<T> o)
        {
            return o.Select(x => Unit.Default);
        }

        /// <summary>
        /// Merge Error and Completion notifications from another observable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="completion"></param>
        /// <returns></returns>
        public static IObservable<T> MergeNotification<T>(this IObservable<T> first, IObservable<Unit> completion)
        {
            return Observable.Merge(
                first.Materialize(),
                completion.Materialize().Select(notif =>
                {
                    switch (notif.Kind)
                    {
                        case NotificationKind.OnCompleted:
                            return Notification.CreateOnCompleted<T>();
                        case NotificationKind.OnError:
                            return Notification.CreateOnError<T>(notif.Exception);
                        default:
                            throw new Exception("Invalid notif type");
                    }
                })
            ).Dematerialize();
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
        public static Scalar Cvt(this Vec3f v)
        {
            return new Scalar(v.Item0, v.Item1, v.Item2);
        }
        public static Scalar Cvt(this Vec3b v)
        {
            return new Scalar(v.Item0, v.Item1, v.Item2);
        }
        public static  ConnectedComponents.Blob FindBiggestBlob(Mat src)
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
        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }
        public static System.Drawing.Color SysBgr255(this Scalar s)
        {
            return System.Drawing.Color.FromArgb(blue:(int)s.Val0, green:(int)s.Val1, red:(int)s.Val2);
        }
        public static Rect RectAround(Point initial , Point final)
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
        public static int Area(this Rect r)
        {
            return r.Width * r.Height;
        }
        public static Point RandomWithin(this Rect r)
        {
            return new Point(rng.Next(r.Left, r.Right), rng.Next(r.Top, r.Bottom));
        }
        public static Scalar cv3(this System.Drawing.Color c)
        {
            return Scalar.FromRgb(c.R, c.G, c.B);
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

        public static double RelativeAngle(this double a, double b)
        {
            var diff =( b.ConfineAngle() - a.ConfineAngle()).ConfineAngle();
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

        public static Rect Cv(this System.Drawing.Rectangle p)
        {
            return new Rect(p.Location.Cv(), p.Size.cv());
            }

        public static System.Drawing.Rectangle Union(this System.Drawing.Rectangle a, System.Drawing.Rectangle b)
        {
            var r= new System.Drawing.Rectangle
            {
                X=Math.Min(a.X,b.X),    
                Y=Math.Min(a.Y,b.Y),    
            };
            r.Width = Math.Max(a.Right, b.Right) - r.Left;
            r.Height = Math.Max(a.Bottom, b.Bottom) - r.Top;
            return r;
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
        public static Point ReadPoint()
        {
            Console.Write("x:");
            int x = int.Parse(Console.ReadLine());
            Console.Write("y:");
            int y = int.Parse(Console.ReadLine());
            return new Point(x, y);
        }
        public static Rect ImgRect(this Mat m)
        {
            return new Rect(Origin, m.Size());
        }
        public static Point2d Vec(this double angle, double mag = 1)
        {
            return new Point2d(x: Math.Cos(angle) * mag, y: Math.Sin(angle) * mag);
        }
        [System.Diagnostics.DebuggerHidden]
        public static T Expect<T>(this T? t, string assert = "") where T : struct
        {
            if (t == null) Debug.Fail(assert);
            return (T)t;
        }
        private static Random rng = new Random(1);

        public static Rect RectAround(this Point2d p, Size sz)
        {
            return new Rect((p - sz.Center()).Round(), sz);
        }

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
        public static Rect Pad(this Rect r, int sz)
        {
            return new Rect(r.X - sz, r.Y - sz, r.Width + sz * 2, r.Height + sz * 2);
        }
        public static Size Pad(this Size r, int sz)
        {
            return new Size(r.Width + sz * 2, r.Height + sz * 2);
        }

        public static Point2d Cvt(this Point2f p)
        {
            return new Point2d(p.X, p.Y);
        }
        public static Point2d Cvt(this Point p)
        {
            return new Point2d(p.X, p.Y);
        }
        public static double Radians(this double deg)
        {
            return deg*Math.PI/180;
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
            return rad*180/Math.PI;
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
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        public static List<T> ToList<T>(this IEnumerator<T> enumerator)
        {
            return enumerator.ToEnumerable().ToList();
        }

        public static int LowerBound<T>(this List<T> list, Func<T, bool> pred)
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

        public static Point2d[] Corners(this Rect2d r)
        {
            return new Point2d[] { r.TopLeft, r.BottomRight, new Point2d(r.Right, r.Top), new Point2d(r.Left, r.Bottom) };
        }

        public static Point2d Closest(this Point2d p, Point2d[] pp)
        {
            Point2d r = default;
            double mx = double.PositiveInfinity;
            foreach (var x in pp)
                if (x.DistanceTo(p) < mx)
                    r = x;
            return r;
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

        public static Point Origin = new Point(0, 0);

        public static Point2d Norm(this Point2d p)
        {
            return p * (1.0 / p.Length());
        }

        public static Point2d LimitDistance(this Point2d p, double maxDist)
        {
            return p.Norm() * Math.Min(p.DistanceTo(Origin), maxDist);
        }
    }
}

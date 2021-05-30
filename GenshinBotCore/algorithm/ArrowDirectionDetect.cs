using genshinbot.data;
using genshinbot.diag;
using genshinbot.util;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    public class ArrowDirectionDetect
    {
        class Settings
        {

            public double Epsilon { get; set; } = 2;
            public ColorRange ArrowColor { get; set; } = new ColorRange(
                new Scalar(250, 220, 0, 0),
                new Scalar(256, 256, 10, 256)
            );

        }
        Settings db = new Settings();//TODO

        Mat s_thres = new Mat();
        private Mat v_thres = new Mat();
        Mat filtered = new Mat();
        public DbgMat Dbg { get; private set; } = new DbgMat(true);
        ~ArrowDirectionDetect()
        {
            s_thres.Dispose();
            v_thres.Dispose();
            filtered.Dispose();
        }
        Mat tmp = Mat.FromArray(1, 1, 1, 0).Transpose();
        Mat sum = new Mat();
        Mat edges = new Mat();
        Mat tmp4 = new Mat();
        Mat img = new Mat();
        Mat hsv = new Mat();
        Mat arrowMask = new Mat();
        Mat red = new Mat();
        Mat tmp1 = new Mat();
        public double GetAngle(Mat img1)
        {
            try
            {
                Console.WriteLine("getangle begin");


                var bac = new Scalar(253, 240, 0);
                Cv2.Resize(img1, img, default, 2, 2);
                Cv2.Absdiff(img, bac, filtered);
                Cv2.Multiply(filtered, filtered, filtered);

                Cv2.Transform(filtered, sum, tmp);
                Cv2.CvtColor(filtered, hsv, ColorConversionCodes.BGR2HSV);

                //Mat sum1 = new Mat();
                //Cv2.BilateralFilter(sum, sum1, 3, 20, 20);sum = sum1;
                //not green & red
                Cv2.InRange(hsv, new Scalar(0,200,0,0),new Scalar(30,256,256,256), red);
                Cv2.InRange(hsv, new Scalar(216,200,0,0),new Scalar(256,256,256,256), tmp4);
                Cv2.BitwiseOr(red, tmp4, red);
                Cv2.InRange(hsv, new Scalar(56,200,0,0),new Scalar(105,256,256,256), tmp4);
                Cv2.BitwiseOr(tmp4, red, edges);

                var cc1 = edges.ConnectedComponentsEx();
                var center = img.Center().Round();
                var lbl = cc1.Labels[center.X, center.Y];
                cc1.RenderBlobs(tmp1);

                arrowMask.SetTo(Scalar.Black);
                cc1.FilterByLabel(edges, arrowMask, lbl);
                Cv2.BitwiseAnd(arrowMask, red, red);
                Cv2.BitwiseAnd(tmp1, tmp1, tmp1, mask: arrowMask);
                Cv2.Dilate(red, red,null);

                //Cv2.AdaptiveThreshold(sum, edges, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 3, 10);
                //Cv2.InRange(img, db.ArrowColor.Min, db.ArrowColor.Max, filtered);
                CvThread.ImShow("filtered", filtered);
                CvThread.ImShow("arrowMask", arrowMask);
                CvThread.ImShow("red", red);
                CvThread.ImShow("blobs", tmp1);
                //TODO optimize by using Mat
                var contours1 = Cv2.FindContoursAsArray(arrowMask, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                var contours = contours1;
                //var max = contours.Where(x => Cv2.PointPolygonTest(x, edges.Center().Round(), false) >= 0);
                // Debug.Assert(contours.Count() ==1);
                var contour = contours.Single();
                Debug.Assert(Cv2.PointPolygonTest(contour, edges.Center().Round(), false) >= 0);

                contour = Cv2.ApproxPolyDP(contour, db.Epsilon, true);
                //contour = Cv2.ConvexHullIndices(contour).Select(idx => contour[idx]).ToArray();

                Cv2.DrawContours(img, new Point[][] { contour }, 0, Scalar.Blue);
                Dbg.Image(img);
                var cc = Cv2.ConnectedComponentsEx(red);
                var lst = cc.Blobs.Skip(1);
                var largest = lst.First();
                foreach(var k in lst)
                {
                    if (k.Area > largest.Area)
                        largest = k;
                }
                var redCtr = largest.Centroid;

                Dbg.Circle(redCtr.Round(), 2, Scalar.Blue);

                double mx = -1;
                Point? p = null;

                foreach (var pt in contour)
                {
                    var d = pt.Cvt().DistanceTo(redCtr);
                    if (d > mx)
                    {
                        mx = d;
                        p = pt;
                    }
                }

                if (p is Point ppp)
                    Dbg.Circle(ppp, 2, Scalar.Red);
                Dbg.Flush();

                Console.WriteLine("getangle end");
            }
            catch(Exception e)
            {
            }
            return 0;//.Expect("Angle unable to be found");
        }

        public static void Test()
        {
            var MinimapLoc = new Rect(53, 15, 189, 189);
            var detect = new ArrowDirectionDetect();
            detect.Dbg.Show();
            var img = Data.Imread("test/arrow_fail.png")[MinimapLoc];
            var angle = detect.GetAngle(img);
            Console.WriteLine(angle);
            Console.ReadLine();
            img = Data.Imread("test/bad arrow.png")[MinimapLoc];
            angle = detect.GetAngle(img);
            Console.WriteLine(angle);
        }
    }
}

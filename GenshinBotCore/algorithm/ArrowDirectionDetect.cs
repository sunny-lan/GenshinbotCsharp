using genshinbot.database;
using OpenCvSharp;
using System;
using System.Collections.Generic;
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
                new Scalar(250, 220, 0),
                new Scalar(256, 256, 10)
            );

        }
        Settings db = new Settings();//TODO

        Mat s_thres = new Mat();
        private Mat v_thres = new Mat();
        Mat filtered = new Mat();

        ~ArrowDirectionDetect()
        {
            s_thres.Dispose();
            v_thres.Dispose();
            filtered.Dispose();
        }

        public double GetAngle(Mat img)
        {
            Cv2.InRange(img, db.ArrowColor.Min, db.ArrowColor.Max, filtered);

            //TODO optimize by using Mat
            var contours = Cv2.FindContoursAsArray(filtered, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            double mx = -1;
            double? angle = null;
            foreach (var contour in contours)
            {
                var approx = Cv2.ApproxPolyDP(contour, db.Epsilon, true);
                var area = Cv2.ContourArea(approx);

                //find the front point of the arrow
                int n = approx.Length;
                for (int idx = 0; idx < n; idx++)
                {
                    var point = approx[idx];
                    var prev = approx[(idx - 1 + n) % n];
                    var next = approx[(idx + 1) % n];

                    //get vectors from prev->cur and next->cur
                    var a = (Point2d)(point - prev);
                    var b = (Point2d)(point - next);

                    //if cur is the front, then the two sides from it should be equal and longest
                    var d1 = a.Length();
                    var d2 = b.Length();
                    if (Math.Abs(d1 - d2) > db.Epsilon) continue;

                    var d = (d1 + d2) / 2.0;

                    if (d > mx)
                    {
                        mx = d;
                        //by adding the vectors from each side, their sideways components cancel
                        //and we get the vector in the direction of the arrow
                        var direction = a + b;
                        angle = Math.Atan2(direction.Y, direction.X);
                    }
                }
            }

            return angle.Expect("Angle unable to be found");
        }
    }
}

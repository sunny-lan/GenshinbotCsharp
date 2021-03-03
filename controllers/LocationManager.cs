using GenshinbotCsharp.database.controllers;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.controllers
{
    class LocationManager
    {
        GenshinBot b;

        public LocationManager(GenshinBot b)
        {
            this.b = b;
            initLocDeduce();
        }
        static Mat erodeKernel = Mat.Ones(3, 3);



        private Mat bigMap;
        private Point2d? approxPos;
        private S4 s4;

        class FilterStage
        {
            public LocationManagerDb db;
            public bool minimap;

            public Mat output = new Mat();
            public Mat mask = new Mat();
            ~FilterStage()
            {

                output.Dispose();
                mask.Dispose();
            }
            public FilterStage(bool minimap, database.controllers.LocationManagerDb db)
            {
                this.db = db;
                this.minimap = minimap;
            }

            public void Filter(Mat orig)
            {
                double amplify = 1;

                using var hsv = orig.CvtColor(ColorConversionCodes.BGR2HSV);
                using var s = hsv.ExtractChannel(1);
                using var v = hsv.ExtractChannel(2);


                Cv2.Laplacian(v, output, MatType.CV_32F, scale: amplify);

                if (minimap)
                {

                    //blot out teleporters etc
                    Cv2.Threshold(s, mask, 10, 255, ThresholdTypes.Binary);
                    Cv2.Erode(mask, mask, erodeKernel);
                    mask.ConvertTo(mask, MatType.CV_32F, 1 / 255.0);
                    Cv2.Multiply(mask, output, output);

                    //blot out arrow
                    Cv2.Circle(output, output.Center().ToPoint(), db.PlayerArrowRadius, Scalar.All(0), -1);
                }
            }
        }

        class LogPolarStage
        {
            //the set of algorithms under algorithm.*
            //are designed to reuse memory when given images of the same size every time
            //however they still work if the image size changes
            //TODO add destructors for all of them

            FilterStage filter;
            algorithm.Hanning hanning = new algorithm.Hanning();
            algorithm.Padding padding = new algorithm.Padding();
            public algorithm.FFT fft = new algorithm.FFT();
            public algorithm.LogPolar logPolar = new algorithm.LogPolar();

            public LogPolarStage(bool minimap, LocationManagerDb db)
            {
                filter = new FilterStage(minimap, db);
            }

            public void Stage1(Mat input)
            {
                filter.Filter(input);
                hanning.Window(filter.output);
                {
                    var windowed = filter.output;
                    int pw = Cv2.GetOptimalDFTSize(windowed.Width);
                    int ph = Cv2.GetOptimalDFTSize(windowed.Height);
                    var sz = new Size(pw, ph);
                    padding.Pad(windowed, sz);

                }
                fft.Run(padding.output);
            }

            public void Stage2(Size sz)
            {
                logPolar.Run(fft.output, sz);
            }
        }


        List<double> scales;
        List<ScaleMatchAlg> scaleAlgs;
        class ScaleMatchAlg
        {
            LogPolarStage miniP, bigP;
            algorithm.Hanning logPolarHanning = new algorithm.Hanning();
            Mat empty = new Mat();

            public ScaleMatchAlg(LocationManagerDb db)
            {
                miniP = new LogPolarStage(true, db);
                bigP = new LogPolarStage(false, db);
            }

            public void Run(Mat big, Mat mini, out double sAResp, out double angle, out double scale)
            {
                miniP.Stage1(mini);
                bigP.Stage1(big);


                Size sz = new Size(
                       Math.Max(miniP.fft.output.Width, bigP.fft.output.Width),
                       Math.Max(miniP.fft.output.Height, bigP.fft.output.Height) * Math.PI
                   );
                bigP.Stage2(sz);
                miniP.Stage2(sz);
                logPolarHanning.Window(miniP.logPolar.output);
                logPolarHanning.Window(bigP.logPolar.output);


                var res = Cv2.PhaseCorrelate(bigP.logPolar.output, miniP.logPolar.output, empty, out sAResp);

                //we need to perform correlation 

                angle = 360.0 * res.Y / sz.Height;


                double klog = sz.Width / Math.Log(miniP.logPolar.radius);
                scale = Math.Exp(res.X / klog);
            }
        }
        void initLocDeduce()
        {
            var db = b.Db.LocationManagerDb;
            scales = new List<double>();
            scaleAlgs = new List<ScaleMatchAlg>();
            for (double s = db.MinScale; s < db.MaxScale * db.ScaleStep; s *= db.ScaleStep)
            {
                scales.Add(s);
                scaleAlgs.Add(new ScaleMatchAlg(db));
            }

        }

        private const double MIN_ACCEPTABLE_PC_RESP = 0.1;

        class S4
        {
            private LocationManagerDb db;
            FilterStage bigF, miniF;
            algorithm.StandardPhaseCorr phaseCorr = new algorithm.StandardPhaseCorr();
            public Point2d? bigMapPos;
            Mat bigMap;
            double scale;

            public S4(LocationManagerDb db, double scale)
            {
                this.db = db;
                this.scale = scale;

                bigF = new FilterStage(false, db);
                miniF = new FilterStage(true, db);
            }

            public void UpdateBigSub(Mat minimap, Point2d newApproxPos)
            {
                var bigSz = minimap.Size().Scale(scale).Pad(db.BigPadding);
                bigMapPos = newApproxPos;
                var bigRect = bigMapPos.Expect().RectAround(bigSz).Intersect(minimap.ImgRect());
                var big = bigMap[bigRect];
                bigF.Filter(big);
                phaseCorr.a.Set(bigF.output);
            }

            /// <summary>
            /// Returns the offset from the center of the big image (bigMapPos)
            /// to the detected player position, in big map coordinates
            /// </summary>
            /// <param name="minimap"></param>
            /// <param name="response"></param>
            /// <returns></returns>
            public Point2d Match(Mat minimap, out double response)
            {
                var miniScaled = minimap.Resize(default, fx: scale, fy: scale);
                miniF.Filter(miniScaled);
                phaseCorr.b.Set(miniF.output);
                return phaseCorr.Run(out response);
            }

        }

        Point2d getLoc(Mat minimap, out double response)
        {
            var db = b.Db.LocationManagerDb;
            var r=s4.Match(minimap, out response);
            return db.Coord2Minimap.Inverse(r + s4.bigMapPos.Expect());
        }

        public Point2d DeduceLocation(screens.PlayingScreen p)
        {
            var db = b.Db.LocationManagerDb;
            Mat minimap = p.SnapMinimap();
            bool approxScaleCalculated = false;
            bool approxLocCalculated = false;

        begin:
            //TODO support starting from MapScreen

            //check map to find initial location
            if (this.approxPos == null)
            {
                var m = p.OpenMap();

                m.UpdateScreenshot();
                var screenCenter = b.W.GetBounds().Cv().Center();
                this.approxPos = m.GetLocation().ToCoord(screenCenter);

                m.Close();

                approxLocCalculated = true;
                approxScaleCalculated = false;
            }

            //convert coord to minimap pos
            var bigApproxPos = db.Coord2Minimap.Transform(this.approxPos.Expect());



            if (this.s4 == null)
            {
                //TODO better support scales <1
                for (int idx = 0; idx < scales.Count; idx++)
                {
                    double s = scales[idx];

                    var scaledMini1 = minimap.Resize(default, fx: s, fy: s);

                    //get subimage in minimap big image
                    var scaRot = bigApproxPos.RectAround(scaledMini1.Size());
                    var big1 = bigMap[scaRot.Intersect(bigMap.ImgRect())];

                    scaleAlgs[idx].Run(big1, scaledMini1, out double sAResp, out double angle, out double scale);

                    if (sAResp < MIN_ACCEPTABLE_PC_RESP)
                    {
                        continue;
                    }

                    Debug.Assert(Math.Abs(angle) < 2); //for now expect angle is 0

                    var actualScale = s * scale;

                    S4 newS4 = new S4(db, actualScale);
                    newS4.UpdateBigSub(minimap, bigApproxPos);
                    var xy = getLoc(minimap, out double response);

                    if (response >= MIN_ACCEPTABLE_PC_RESP)
                    {
                        //TODO
                        Debug.Assert(xy.DistanceTo(approxPos.Expect())<10);
                        this.s4 = newS4;
                        return xy;
                    }
                }

                if (s4 == null)
                {
                    //we were unable to find a valid scale
                    //this means
                    if (approxLocCalculated)
                    {
                        //a) the algorithm failed or the Coord2Minimap setting is wrong
                        throw new Exception("Failed to find valid scale");
                    }
                    else
                    {
                        //b) the value of ApproxPos is out of date
                        this.approxPos = null; //invalidate and try again
                        goto begin;
                    }
                }

                approxScaleCalculated = true;
            }

            //get subimage
            var r = getLoc(minimap, out double response1);
            if (response1 < MIN_ACCEPTABLE_PC_RESP)
            {
                if (approxScaleCalculated)
                {
                    //If we had already tried to calculate scale, and still
                    // couldn't find position, then the algorithm sucks
                    throw new Exception("Failed to find valid position");
                }
                else
                {
                    //try invalidating scale and calculating again
                    this.s4 = null;
                    goto begin;
                }
            }

            //update approx pos with 
            approxPos = r;

            //if we have moved too far off from the current minimap matcher,
            // we need to update it
            if (r.DistanceTo(s4.bigMapPos.Expect()) > 30)
            {
                s4.UpdateBigSub(minimap, r);
            }

            return r;
        }

        ~LocationManager()
        {
            bigMap.Dispose();
        }
    }
}

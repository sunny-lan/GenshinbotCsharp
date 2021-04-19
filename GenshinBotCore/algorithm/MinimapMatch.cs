using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm.MinimapMatch
{
    public class Settings
    {
        public virtual Mat BigMap { get; set; }
        public double MinScale { get; set; } = 1;
        public double MaxScale { get; set; } = 3;
        public double ScaleStep { get; set; } = 1.3;

        public int PlayerArrowRadius { get; set; } = 20;
        public int BigPadding { get; set; } = 25;
        public double MinAcceptableCorrelationResponse { get; set; } = 0.15;
        public double MaxDistanceBeforeTrackerMoves { get; set; } = 30;
        public bool AutoUpdateTrackerWindowPos { get; set; } = true;
        public double MaxDistanceFromApproxToActual { get; set; } = 100;



    }
    public class PositionMatcher
    {

        internal Settings db;

        FilterStage bigF, miniF;
        StandardPhaseCorr phaseCorr = new StandardPhaseCorr();

        public double Scale { get; private set; }

        /// <summary>
        /// We are not matching against the entire minimap, but just a small area
        /// around the last known position
        /// </summary>
        public Point2d SubImageCenterPos;
        //subimage is lazily calculated whenever it is needed
        Rect? lastRect;
        public PositionMatcher(Settings db, double scale, Point2d approxPos)
        {
            this.db = db;
            this.Scale = scale;

            bigF = new FilterStage(false, db);
            miniF = new FilterStage(true, db);
            SubImageCenterPos = approxPos;
        }

        public void UseSubImg(Point2d pos)
        {
            lastRect = null;
            SubImageCenterPos = pos;
        }

        Mat subImg;

        public Point2d? Match(Mat minimap)
        {
            var bigSz = minimap.Size().Scale(Scale).Pad(db.BigPadding);
            var subRect = SubImageCenterPos.RectAround(bigSz);
            var bounds = db.BigMap.ImgRect();
            subRect = subRect.Intersect(bounds);
            if ( lastRect != subRect)
            {
                Console.WriteLine("big rect change");
                subImg = db.BigMap[subRect];
                lastRect = subRect;
                bigF.Filter(subImg);
                phaseCorr.b.Set(bigF.output);
            }

            //perform scaling, filtering
            using var miniScaled = minimap.Resize(default, fx: Scale, fy: Scale);

            miniF.Filter(miniScaled);

            //run phase correlation
            phaseCorr.a.Set(miniF.output);
            var translation = phaseCorr.Run(out double response);

            //check if result is valid
            if (response < db.MinAcceptableCorrelationResponse)
                return null;

            //  top left of subimage -> top left of minimap -> center of minimap
            return subRect.TopLeft + translation + miniScaled.Center();
        }
    }

    public class PositionTracker
    {
        private Settings db;

        PositionMatcher m;

        Point2d? lastKnownPos;

        public PositionTracker( PositionMatcher m)
        {
            this.db = m.db;
            this.m = m;
            lastKnownPos = m.SubImageCenterPos;
        }

        public void UpdatePos(Point2d newPos)
        {
            lastKnownPos = m.SubImageCenterPos;
            m.UseSubImg(newPos);
        }

        /// <summary>
        /// Returns the offset from the center of the big image (bigMapPos)
        /// to the detected player position, in big map coordinates
        /// </summary>
        /// <param name="minimap"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public Point2d? Track(Mat minimap)
        {
            lastKnownPos.Expect("Tried to call Match on a PositionTracker which has lost tracking");

            var res = m.Match(minimap);
            if (res == null)
            {
                lastKnownPos = null;
                return null;
            }

            var pos = res.Expect();

            //if we have moved too much to the edges of the bigmap subregion
            // we need to recalculate the bigmap subregion (above)
            if (db.AutoUpdateTrackerWindowPos &&
                pos.DistanceTo(m.SubImageCenterPos) > db.MaxDistanceBeforeTrackerMoves)
            {
                m.UseSubImg(pos);
                Console.WriteLine("Tracker update");
            }

            lastKnownPos = pos;
            return pos;
        }

    }

    class FilterStage
    {
        static Mat erodeKernel = Mat.Ones(3, 3);

        public Settings db;
        public bool minimap;

        public Mat output = new Mat();
        public Mat mask = new Mat();
        ~FilterStage()
        {
            
            output.Dispose();
            mask.Dispose();
        }
        public FilterStage(bool minimap, Settings db)
        {
            this.db = db;
            this.minimap = minimap;
        }

        public void Filter(Mat orig)
        {
            double amplify = 1;

            using var hsv = orig.CvtColor(ColorConversionCodes.BGR2HSV);
            //TODO use CV.ExtractChannel
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
                Cv2.Circle(output, output.Center().Round(), db.PlayerArrowRadius, Scalar.All(0), -1);
            }
        }
    }

    class LogPolarStage
    {

        FilterStage filter;
        algorithm.Hanning hanning = new algorithm.Hanning();
        algorithm.Padding padding = new algorithm.Padding();
        public algorithm.FFT fft = new algorithm.FFT();
        public algorithm.LogPolar logPolar = new algorithm.LogPolar();

        public LogPolarStage(bool minimap, Settings db)
        {
            filter = new FilterStage(minimap, db);
        }

        public void Stage1(Mat input)
        {
            filter.Filter(input);
            hanning.Window(filter.output);
            var windowed = filter.output;
            int pw = Cv2.GetOptimalDFTSize(windowed.Width);
            int ph = Cv2.GetOptimalDFTSize(windowed.Height);
            var sz = new Size(pw, ph);
            padding.Pad(windowed, sz);
            fft.Run(padding.output, nonZeroRows: windowed.Rows);
        }

        public void Stage2(Size sz)
        {
            logPolar.Run(fft.output, sz);
        }
    }

    public class ScaleMatcher
    {
        class ScaleMatchAlg
        {
            LogPolarStage miniP, bigP;
            algorithm.Hanning logPolarHanning = new algorithm.Hanning();
            static Mat empty = new Mat();

            public ScaleMatchAlg(Settings db)
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


        private Settings db;
        List<double> scales;
        List<ScaleMatchAlg> scaleAlgs;

        public ScaleMatcher(Settings db)
        {
            this.db = db;
            scales = new List<double>();
            scaleAlgs = new List<ScaleMatchAlg>();
            for (double s = db.MinScale; s < db.MaxScale * db.ScaleStep; s *= db.ScaleStep)
            {
                scales.Add(s);
                scaleAlgs.Add(new ScaleMatchAlg(db));
            }
        }

        public Point2d? FindScale(Point2d approxPos, Mat minimap, out PositionMatcher matcher)
        {
            matcher = null;
            var bigMap = db.BigMap;
            for (int idx = 0; idx < scales.Count; idx++)
            {
                double s = scales[idx];

                var scaledMini1 = minimap.Resize(default, fx: s, fy: s);

                //get subimage in minimap big image
                var scaRot = approxPos.RectAround(scaledMini1.Size());
                var big1 = bigMap[scaRot.Intersect(bigMap.ImgRect())];

                scaleAlgs[idx].Run(big1, scaledMini1, out double sAResp, out double angle, out double scale);

                if (sAResp < db.MinAcceptableCorrelationResponse)
                {
                    continue;
                }

                //TODO fails when walking over water
                Debug.Assert(Math.Abs(angle) < 2); //for now expect angle is 0

                var actualScale = s * scale;

                PositionMatcher testMatcher = new PositionMatcher(db, actualScale, approxPos);
                var xy1 = testMatcher.Match(minimap);

                if (xy1 is Point2d xy)
                {
                    //We expect the position returned to be close to the approx position
                    Debug.Assert(xy.DistanceTo(approxPos) < db.MaxDistanceFromApproxToActual);
                    matcher = testMatcher;
                    return xy;
                }
            }
            return null;
        }
    }

    class ScaleInvTracker
    {
        
    }
}

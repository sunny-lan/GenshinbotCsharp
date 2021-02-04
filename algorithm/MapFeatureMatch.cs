using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace GenshinbotCsharp.algorithm
{
    class MapFeatureMatch
    {
        //TODO switch this to included one
        class ConnectedComponents
        {
            public enum StatIdx : int
            {
                CC_STAT_AREA = 4,
                CC_STAT_HEIGHT = 3,
                CC_STAT_LEFT = 0,
                CC_STAT_MAX = 5,
                CC_STAT_TOP = 1,
                CC_STAT_WIDTH = 2,
            }
            public struct Stats
            {

                public int Left, Top, Width, Height;
                public int Area;

                // public double cx, cy;
                public Rect Rect => new Rect(Left, Top, Width, Height);
                public int Bottom => Top + Height;
                public int Right => Left + Width;
                public Point P1 => new Point(Left, Top);
                public Point P2 => new Point(Right, Bottom);


                public Size Size => new Size(Width, Height);

            }

            Mat labels = new Mat(),
                stats = new Mat(),
                centroids = new Mat();
            public int Count { get; private set; } = -1;

            public void CalculateFrom(Mat img)
            {

                Count = Cv2.ConnectedComponentsWithStats(img, labels, stats, centroids);

            }

            public Stats this[int idx]
            {
                get
                {
                    return new Stats
                    {
                        Left = this[idx, StatIdx.CC_STAT_LEFT],
                        Top = this[idx, StatIdx.CC_STAT_TOP],
                        Width = this[idx, StatIdx.CC_STAT_WIDTH],
                        Height = this[idx, StatIdx.CC_STAT_HEIGHT],
                        Area = this[idx, StatIdx.CC_STAT_AREA],
                    };
                }
            }

            public int this[int idx, StatIdx type]
            {
                get
                {
                    if (Count == -1)
                        throw new Exception("must call CalculateFrom before accessing stats");
                    if (idx >= Count)
                        throw new IndexOutOfRangeException();

                    return stats.Get<int>(idx, (int)type);
                }
            }

            ~ConnectedComponents()
            {
                labels.Dispose();
                stats.Dispose();
                centroids.Dispose();
            }
        }


        class Template
        {
            public double SatMinThres;
            //public double ValMinThres;

            public Mat Mask;
            public Mat UnweightedMask;

           // public Mat Value
            public Mat Sat;
            //public Mat Hue;

            //public double ValueAvg;

            public Mat Filter;

            public ConnectedComponents.Stats Stats;

            ConnectedComponents components = new ConnectedComponents();
            public Template(string path, string pathAlpha)
            {
                Filter = new Mat();
                Mask = new Mat();

                //1. calculate filter image and components
                using var orig = Cv2.ImRead(path);

                Cv2.CvtColor(orig, orig, ColorConversionCodes.BGR2HSV);

                Sat = orig.ExtractChannel(1);//s
                Cv2.Threshold(Sat, Filter, 15, 255, ThresholdTypes.BinaryInv);

                components.CalculateFrom(Filter);
                if (components.Count != 2)
                    throw new NotImplementedException("split component template not supported yet");

                Stats = components[1];

                //3. calculate image used for masking (alpha channel)
                using var origAlpha = Cv2.ImRead(pathAlpha, ImreadModes.Unchanged);
                using var UnweightedMask = origAlpha[Stats.Rect].ExtractChannel(3);
                UnweightedMask.ConvertTo(Mask, MatType.CV_32F);

                //2. calculate image used for template matching (value channel)
                //var subImg = orig[Stats.Rect];
                //Value = subImg.ExtractChannel(2);
                //Hue = subImg.ExtractChannel(0);

                Sat = Sat[Stats.Rect];
                //Filter = Filter[Stats.Rect];
                //ValueAvg = Value.Mean(Filter)[0];
                //SatAvg = Sat.Mean(Filter)[0];
            }

            ~Template()
            {
                Mask.Dispose();
                UnweightedMask.Dispose();
                Sat.Dispose();
                Filter.Dispose();
            }

            public static Template Waypoint()
            {
                return new Template(Data.Get("map/icons/waypoint_1680x1050.PNG"), Data.Get("map/icons/waypoint_1680x1050_alpha.PNG"))
                {
                    SatMinThres = 0.1,
                   //ValMinThres = 0.1,
                };
            }
        }

        private ConnectedComponents components = new ConnectedComponents();
        private Template waypoint;
        private Mat matchResult = new Mat();
        private Mat matchResult2 = new Mat();

        public MapFeatureMatch()
        {
            waypoint = Template.Waypoint();
            Precalculate();
        }

        class Pair : IComparable<Pair>
        {
            public double Angle;
            public Feature A;
            public Feature B;

            public int CompareTo(Pair other)
            {
                return Angle.CompareTo(other.Angle);
            }

            public Pair(Feature a, Feature b)
            {
                A = a;
                B = b;
                var diff = b.Coordinates - a.Coordinates;
                Angle = Atan2(diff.Y, diff.X);
            }

            public double Distance => A.Coordinates.DistanceTo(B.Coordinates);
        }

        private List<Pair> allPairs = new List<Pair>();

        private void Precalculate()
        {
            var t = Data.Map.Teleporters;
            for (int i = 0; i < t.Count; i++)
            {

                var a = t[i];
                // if(a.Name.ToLower().Contains("aiozang") || a.Name== "Qingyun Peak Teleporter")
                for (int j = i + 1; j < t.Count; j++)
                {
                    var b = t[j];
                    // if (b.Name.ToLower().Contains("aiozang") || b.Name == "Qingyun Peak Teleporter")
                    {
                        var pair = new Pair(a, b);
                        if (pair.Angle < 0)
                            pair = new Pair(b, a);
                        if (pair.Angle < 0)
                            throw new Exception("assert failed");
                        allPairs.Add(pair);
                    }
                }
            }

            allPairs.Sort();
        }


        Mat debugImg = new Mat();
        Mat hsv = new Mat();

        //things which are definitely not matches
        const double ANGLE_MATCH_TOLERANCE = 0.2;
        const double DISTANCE_MATCH_TOLERANCE = 0.5;
        private const double RATIO_TEST_TOLERANCE = 2;

        private double scoreFunc(double a, double b)
        {
            return 1.0 / (1 + Abs(a - b));
        }

        //Mat t_sub_v = new Mat();
        //Mat t_sub_h = new Mat();
        Mat t_sub_mask = new Mat();
        //Mat t_unweighted_mask = new Mat();
        Mat filter = new Mat();
        Mat t_sat = new Mat();
        public IEnumerable<Point2d> FindTeleporters(Mat buf)
        {
            buf.CopyTo(debugImg);

            Template template = waypoint;


            Cv2.CvtColor(buf, hsv, ColorConversionCodes.BGR2HSV);

            using var sat = hsv.ExtractChannel(1);
            Cv2.Threshold(sat, filter, 15, 255, ThresholdTypes.BinaryInv);

            components.CalculateFrom(filter);

            int iw = hsv.Width, ih = hsv.Height;

            var matches = new List<Tuple<double, Point2d>>();

            //sort candidate areas using heuristic
            //compares the size of the template image with the size of the  actual area of interest
            var candidates = new List<Tuple<double, int>>();
            for (int i = 1; i < components.Count; i++)
            {
                ConnectedComponents.Stats c = components[i];

                //ignore subimages which are way too small
                if (c.Area < template.Stats.Area / 6
                    || c.Width < template.Stats.Width / 3
                    || c.Height < template.Stats.Height / 3
                    ) continue;
                //ignore subimages which are way too big
                if (c.Area > template.Stats.Area * 20
                    || c.Width > template.Stats.Width * 10
                    || c.Height > template.Stats.Height * 10
                    ) continue;

                double areaScale = Sqrt(template.Stats.Area / (double)c.Area);
                double sizeDiff = Sqrt(scoreFunc(areaScale * c.Width, template.Stats.Width) *
                                        scoreFunc(areaScale * c.Height, template.Stats.Height));
                double ratioDiff = scoreFunc(template.Stats.Width / template.Stats.Height,
                    c.Width / c.Height);
                candidates.Add(new Tuple<double, int>(ratioDiff, i));
            }
            candidates.Sort();


            foreach (var state in candidates)
            {
                var i = state.Item2;
                ConnectedComponents.Stats c = components[i];


                //resize the template to fit within given image
                double maxFactor = Min(1.0, Min(c.Width /(double) template.Stats.Width, c.Height / (double)template.Stats.Height));
                var t_size = new Size(maxFactor*template.Stats.Width, maxFactor*template.Stats.Height);
                //Cv2.Resize(template.Value, t_sub_v, t_size);
                // Cv2.Resize(waypoint.Hue, t_sub_h, size);
                Cv2.Resize(template.Mask, t_sub_mask, t_size);
                // Cv2.Resize(waypoint.UnweightedMask, t_unweighted_mask, size);
                Cv2.Resize(template.Sat, t_sat, t_size);

                //select subimage
                Mat sub = hsv[c.Rect];

                //images could be too small after clipping off the side of the screenshot
                if (sub.Width < t_sub_mask.Width || sub.Height < t_sub_mask.Height)
                    continue;

                //match the templates using the hue and value channel
               // using Mat value = sub.ExtractChannel(2);//value

                //adjust brightness
                //Mat sub_filter = filter[c.Rect];
                //double brightnessAdjust = waypoint.ValueAvg - value.Mean(sub_filter)[0];
               // Cv2.Add(value, brightnessAdjust, value);

                //Cv2.PutText(debugImg, "v:" + brightnessAdjust, new Point(c.Right, c.Bottom), HersheyFonts.HersheyPlain, 1, Scalar.Pink, thickness: 2);

               // Cv2.MatchTemplate(value, t_sub_v, matchResult2, TemplateMatchModes.SqDiffNormed, t_sub_mask);


                //using Mat hue = sub.ExtractChannel(0);//hue
                //Cv2.MatchTemplate(hue, t_sub_h, matchResult, TemplateMatchModes.SqDiffNormed, t_sub_mask);
                //matchResult.MinMaxLoc(out var hueScore, out var _, out Point hueLoc, out Point _);

                Mat sub_sat = sat[c.Rect];
                Cv2.MatchTemplate(sub_sat, t_sat, matchResult, TemplateMatchModes.SqDiffNormed, t_sub_mask);
                //Cv2.Add(matchResult2, 1.0, matchResult2);
                //Cv2.Add(matchResult, 1.0, matchResult);
                //Cv2.Multiply(matchResult, matchResult2, matchResult);

                matchResult.MinMaxLoc(out var score, out var _, out Point loc, out Point _);


                Point2d bestPoint = loc;
                Rect area = new Rect(
                    new Point(c.Left + bestPoint.X, c.Top + bestPoint.Y),
                    t_size
                );

                double deviation =  matchResult.Mean()[0]-score;


                if (score < 0.1)
                {
                    debugImg.PutText("s:" + Round(score, 3)
                            + "d:" + Round(deviation, 3),
                        area.TopLeft, HersheyFonts.HersheyPlain, fontScale: 1, color: Scalar.Red, thickness: 2);

                    Cv2.Rectangle(img: debugImg, area, color: Scalar.Blue, thickness: 2);


                    var mapPoint = new Point((area.Left + area.Right) / 2.0, area.Bottom);
                    Cv2.Circle(img: debugImg, center: mapPoint, radius: 2, color: Scalar.Green, thickness: 2);


                    yield return mapPoint;
                }
            }

            Cv2.ImShow("de",debugImg);

        }
        double anglediff(double a, double b)
        {
            var diff = Abs(a - b);
            return Min(diff, 2 * PI - diff);
        }
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        public void FindLocation(Mat buf)
        {

            var list = FindTeleporters(buf).GetEnumerator();



            //get the first two teleporters
            bool flag = false;
            Point2d a = default, b = default;
            if (list.MoveNext())
            {
                a = list.Current;
                if (list.MoveNext())
                {
                    b = list.Current;
                    flag = true;
                }
            }


            Cv2.NamedWindow("result", WindowMode.KeepRatio);
            if (flag)
            {
                //calculate the angle between the two
                var diff = b - a;
                var angle = Atan2(diff.Y, diff.X);
                if (angle < 0)
                {
                    Swap(ref b, ref a);
                    diff = b - a;
                    angle = Atan2(diff.Y, diff.X);
                }

                //find all pairs of teleporters with a similar angle
                var candidates = new List<Pair>();
                foreach (var x in allPairs)
                {

                    // if (anglediff(x.Angle, angle) < ANGLE_MATCH_TOLERANCE)
                    {
                        candidates.Add(x);
                    }
                }

                //look for more teleporters until we can narrow down the possible transformations to 1

                while (candidates.Count > 1)
                {
                    if (!list.MoveNext())
                        throw new Exception("failed to narrow down!");
                    var teleporterPos = list.Current;

                    Cv2.ImShow("result", debugImg);
                    Cv2.WaitKey();
                    var nextCandiates = new List<Pair>();

                    //narrow down list of possible candidates
                    foreach (var candidate in candidates)
                    {
                        //calculate the transformation of this candidate
                        //conversion from screen to map 
                        double scale = candidate.Distance / a.DistanceTo(b);
                        Point2d translation = candidate.A.Coordinates - a * scale;

                        //transform the found teleporter position to map coordinates
                        Point2d mapCoords = teleporterPos * scale + translation;

                        //search points for one which will match this teleporter
                        foreach (var teleporterM in Data.Map.Teleporters)
                        {
                            if (teleporterM.Coordinates.DistanceTo(mapCoords) < DISTANCE_MATCH_TOLERANCE)
                            {
                                nextCandiates.Add(candidate);
                                break;
                            }
                        }

                    }

                    candidates = nextCandiates;
                }

                if (candidates.Count == 0)
                    throw new Exception("no candidates!");

                double scaleFinal = candidates[1].Distance / a.DistanceTo(b);
                Point2d translationFinal = candidates[1].A.Coordinates - a * scaleFinal;

                foreach (var teleporterM in Data.Map.Teleporters)
                {
                    Point2d screenCoords = (teleporterM.Coordinates - translationFinal) * (1 / scaleFinal);
                    Cv2.PutText(debugImg, "d: " + teleporterM.Name,
                        new Point(screenCoords.X, screenCoords.Y),
                        HersheyFonts.HersheyPlain,
                        fontScale: 1, color: Scalar.Red, thickness: 2);
                }

            }

            Cv2.ImShow("result", debugImg);
            Cv2.WaitKey(1);


        }

        ~MapFeatureMatch()
        {
            matchResult.Dispose();
            debugImg.Dispose();
        }

    }
}

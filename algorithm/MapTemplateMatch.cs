using GenshinbotCsharp.database.map;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace GenshinbotCsharp.algorithm
{
     class MapTemplateMatch
    {

        public interface Template { }
        public class TemplateSat:Template
        {
            public FeatureType FeatureType;
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
            public TemplateSat(string path, string pathAlpha)
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
                UnweightedMask = origAlpha[Stats.Rect].ExtractChannel(3);
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

            ~TemplateSat()
            {
                Mask.Dispose();
                UnweightedMask.Dispose();
                Sat.Dispose();
                Filter.Dispose();
            }

            public static TemplateSat Waypoint()
            {
                return new TemplateSat(Data.Get("map/icons/waypoint_1680x1050.PNG"), Data.Get("map/icons/waypoint_1680x1050_alpha.PNG"))
                {
                    SatMinThres = 0.1,
                   //ValMinThres = 0.1,
                   FeatureType=FeatureType.Teleporter,
                };
            }
        }


        public MapTemplateMatch()
        {
            waypoint = TemplateSat.Waypoint();
        }



        private ConnectedComponents components = new ConnectedComponents();
        private TemplateSat waypoint;
        private Mat matchResult = new Mat();
        //private Mat matchResult2 = new Mat();
        Mat hsv = new Mat();
        //Mat t_sub_v = new Mat();
        //Mat t_sub_h = new Mat();
       // Mat t_sub_mask = new Mat();
        //Mat t_unweighted_mask = new Mat();
        Mat filter = new Mat();
        //Mat t_sat = new Mat();

        public class Result
        {
            public Point2d Point;
            public TemplateSat Match;
            public Rect BoundingBox;
            public double Score;
        }

        public IEnumerable<Result> FindTeleporters(Mat buf)
        {
            buf.CopyTo(Debug.img);

            TemplateSat template = waypoint;


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
                double sizeDiff = Sqrt(Util.scoreFunc(areaScale * c.Width, template.Stats.Width) *
                                        Util.scoreFunc(areaScale * c.Height, template.Stats.Height));
                double ratioDiff = Util.scoreFunc(template.Stats.Width / template.Stats.Height,
                    c.Width / c.Height);
                candidates.Add(new Tuple<double, int>(ratioDiff, i));
            }
            candidates.Sort();


            foreach (var state in candidates)
            {
                var i = state.Item2;
                ConnectedComponents.Stats c = components[i];


                //resize the template to fit within given image
                //double maxFactor = Min(1.0, Min(c.Width /(double) template.Stats.Width, c.Height / (double)template.Stats.Height));
                //var t_size = new Size(maxFactor*template.Stats.Width, maxFactor*template.Stats.Height);
                //Cv2.Resize(template.Value, t_sub_v, t_size);
                // Cv2.Resize(waypoint.Hue, t_sub_h, size);
                //Cv2.Resize(template.Mask, t_sub_mask, t_size);
                var t_sub_mask = template.Mask;
                var t_sat = template.Sat;
                // Cv2.Resize(waypoint.UnweightedMask, t_unweighted_mask, size);
                //Cv2.Resize(template.Sat, t_sat, t_size);
                var t_size = template.Stats.Size;

                //select subimage
                //Mat sub = hsv[c.Rect];

                //images could be too small after clipping off the side of the screenshot
                if (c.Width < t_sub_mask.Width || c.Height < t_sub_mask.Height)
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
                    Debug.img.PutText("s:" + Round(score, 3)
                            + "d:" + Round(deviation, 3),
                        area.TopLeft, HersheyFonts.HersheyPlain, fontScale: 1, color: Scalar.Red, thickness: 2);

                    Debug.img.Rectangle( area, color: Scalar.Blue, thickness: 2);


                    var mapPoint = new Point2d((area.Left + area.Right) / 2.0, area.Bottom);
                    Debug.img.Circle( center: mapPoint.ToPoint(), radius: 2, color: Scalar.Green, thickness: 2);


                    yield return new Result
                    {
                        Score = score,
                        BoundingBox = area,
                        Point = mapPoint,
                        Match = template,
                    };
                }
            }

        }
      

        ~MapTemplateMatch()
        {
            matchResult.Dispose();

            filter.Dispose();
            hsv.Dispose();
            matchResult.Dispose();
            
        }

    }
}

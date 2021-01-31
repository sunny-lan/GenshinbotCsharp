using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace GenshinbotCsharp.screens
{
    struct Coordinates
    {
        double longitude, lat;
    }
    class MapScreen
    {
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
            public Mat Mask;
            public Mat Match;//actual template used for match
            Mat filter; //binary image used by findconnectedcomponents
            ConnectedComponents components = new ConnectedComponents();
            public ConnectedComponents.Stats Stats;
            public Template(string path, string pathAlpha)
            {
                //1. calculate filter image and components
                using var orig =  Cv2.ImRead(path);

                Cv2.CvtColor(orig, orig, ColorConversionCodes.BGR2HSV);

                filter = orig.ExtractChannel(1);//s
                Cv2.Threshold(filter, filter, 10, 255, ThresholdTypes.BinaryInv);

                components.CalculateFrom(filter);
                if (components.Count != 2)
                    throw new NotImplementedException("split component template not supported yet");

                Stats = components[1];

                //2. calculate image used for template matching (value channel)
                Match = orig[Stats.Rect].ExtractChannel(2);

                //3. calculate image used for masking (alpha channel)
                using var origAlpha = Cv2.ImRead(pathAlpha,ImreadModes.Unchanged );
                Mask = origAlpha[Stats.Rect].ExtractChannel(3);
                Mask.ConvertTo(Mask, MatType.CV_32F);
            }

            ~Template()
            {
                Mask.Dispose();
                Match.Dispose();
                filter.Dispose();
            }

            public static Template Waypoint()
            {
                return new Template(Data.Get("map/icons/waypoint.PNG"), Data.Get("map/icons/waypoint_alpha.PNG"));
            }
        }


        private GenshinWindow g;
        private Screenshot.Buffer buf;
        private ConnectedComponents components = new ConnectedComponents();
        private Template waypoint;
        private Mat matchResult=new Mat();

        public MapScreen(GenshinWindow g)
        {
            this.g = g;
            waypoint = Template.Waypoint();
            var r = g.GetRect();
            buf = Screenshot.GetBuffer(r.Width, r.Height);


        }

        public void Attach()
        {

        }

        Mat debugImg=new Mat();
        Mat hsv = new Mat();

        public void FindLocation()
        {
            g.TakeScreenshot(0, 0, buf);
            buf.Mat.CopyTo(debugImg);

            Cv2.CvtColor(buf.Mat, hsv, ColorConversionCodes.BGR2HSV);

            using var filter = hsv.ExtractChannel(1);

            Cv2.Threshold(filter, filter, 10, 255, ThresholdTypes.BinaryInv);
            components.CalculateFrom(filter);
            Cv2.ImShow("filt", filter);

            int iw =hsv.Width, ih = hsv.Height;


            for (int i = 1; i < components.Count; i++)
            {
                ConnectedComponents.Stats c = components[i];

                //ignore subimages which are way too small
                if (c.Area < waypoint.Stats.Area / 2
                    || c.Width < waypoint.Stats.Width / 2
                    || c.Height < waypoint.Stats.Height / 2
                    ) continue;

                Mat t_sub = waypoint.Match;
                int tw = t_sub.Width, th = t_sub.Height;

                //calculate biggest possible rect template could be in
                int l = c.Left, r = c.Right, t = c.Top, b = c.Bottom;
                int left = Max(0, Min(r - tw, l));
                int right = Min(iw, Max(l + tw, r));
                int top = Max(0, Min(b - th, t));
                int bottom = Min(ih, Max(t + th, b));

                Mat sub = hsv[top, bottom, left, right];

                //images could be too small after clipping off the side of the screenshot
                if (sub.Width < t_sub.Width || sub.Height < t_sub.Height) 
                    continue;

                //match the templates using the value channel
                using Mat subCh = sub.ExtractChannel(2);//value
                Cv2.MatchTemplate(subCh, waypoint.Match, matchResult, TemplateMatchModes.SqDiffNormed, waypoint.Mask);

                matchResult.MinMaxLoc(out var score, out var _, out Point loc, out Point _);

                Cv2.Rectangle(debugImg, c.Rect, Scalar.Red, thickness: 2);
                if (score < 0.01)
                {
                    Rect area = new Rect(new Point(left, top) + loc, waypoint.Stats.Rect.Size);
                    Cv2.Rectangle(img: debugImg, area, color: Scalar.Blue, thickness: 2);
                }
                
            }
            Cv2.NamedWindow("result", WindowMode.KeepRatio);
            Cv2.ImShow("result", debugImg);
            Cv2.WaitKey(1);
        }

        ~MapScreen()
        {
            matchResult.Dispose();
            debugImg.Dispose();
        }
    }
}

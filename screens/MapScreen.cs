using GenshinbotCsharp.database.map;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Math;

namespace GenshinbotCsharp.screens
{
   
    class MapScreen : Screen
    {
        public class Db
        {

            public class RD
            {
                public Point2d ActionBtnLoc { get; internal set; }
                public Size ActiveArea { get; set; }
            }

            public Dictionary<Size, RD> R { get; set; } = new Dictionary<Size, RD>
            {
                [new Size(1440, 900)] = new RD
                {
                    ActionBtnLoc = new Point2d(1234, 846),
                    ActiveArea = new Size(1000, 500),
                },
                [new Size(1680, 1050)] = new RD
                {
                    ActionBtnLoc = new Point2d(1496, 978),
                    ActiveArea = new Size(1200, 700),
                }
            };
        }

        private GenshinBot b;

        public Mat Map;

        public MapScreen(GenshinBot b)
        {
            this.b = b;

            initLocator();

        }

        public void Close()
        {
            b.W.K.KeyPress(input.GenshinKeys.Map);
            Thread.Sleep(2000);//TODO
            b.S(b.PlayingScreen);
        }

        public bool CheckActive()
        {
            throw new NotImplementedException();
        }


        public algorithm.MapLocationMatch LocationMatch { get; private set; }
        public algorithm.MapTemplateMatch TemplateMatch { get; private set; }

        private void initLocator()
        {
            LocationMatch = new algorithm.MapLocationMatch(b.Db.MapDb.Features);
            TemplateMatch = new algorithm.MapTemplateMatch();
        }

        public void AddFeature(Feature f)
        {
            b.Db.MapDb.Features.Add(f);
            LocationMatch.AddFeature(f);
        }

        private List<algorithm.MapTemplateMatch.Result> features;
        private algorithm.MapLocationMatch.Result location;

        public void UpdateScreenshot()
        {
            Map = b.W.Screenshot(b.W.GetBounds());

            features = null;
            location = null;
        }

        public List<algorithm.MapTemplateMatch.Result> GetFeatures()
        {
            if (features == null)
                features = TemplateMatch.FindTeleporters(Map).ToList();
            return features;
        }

        public bool ExpectUnknown = true;

        public algorithm.MapLocationMatch.Result GetLocation()
        {
            if (location == null)
                location = LocationMatch.FindLocation2(GetFeatures(), b.W.GetSize(), ExpectUnknown);
            return location;
        }

        public void TeleportTo(Feature teleporter)
        {
            var db = b.Db.MapScreenDb;
            Debug.Assert(teleporter.Type == FeatureType.Teleporter);
            var p = ShowOnScreen(teleporter.Coordinates);
            b.W.MouseTo(p);
            b.W.MouseClick(0);
            Thread.Sleep(1000);
            b.W.MouseTo(db.R[b.W.GetSize()].ActionBtnLoc);
            b.W.MouseClick(0);
            b.SWait(b.LoadingScreen);
            b.LoadingScreen.WaitTillDone();
            Thread.Sleep(1000);
            b.S(b.PlayingScreen);
        }

        /// <summary>
        /// Returns the screen point of a coordinate. If the point is not on screen, it will try to move towards the point
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public Point2d ShowOnScreen(Point2d coord)
        {
            var db = b.Db.MapScreenDb;
            while (true)
            {
                UpdateScreenshot();
                var r = b.W.GetBounds();
                var center = r.Center();
                var active = center.RectAround(db.R[r.Size].ActiveArea);

                var l = GetLocation();
                var point = l.ToPoint(coord);

                if (active.Contains(point.Round()))
                {
                    return point;
                }

                //use a randomized draw start location, as the mouse move doesn't work when
                // the drag begin location is on a clickable thing
                var beginPos = active.RandomWithin();

                b.W.I.MouseTo(beginPos);
                Thread.Sleep(10);

                b.W.I.MouseDown(0);
                Thread.Sleep(10);

                b.M.Goto((beginPos - point).LimitDistance(200)+ beginPos).Wait();
                Thread.Sleep(100); //pause to prevent flick gesture from happening

                b.W.I.MouseUp(0);
                Thread.Sleep(10);

            }
        }

        public static void Test()
        {
            GenshinBot b = new GenshinBot();
                Console.ReadKey();
            var m=b.S(b.MapScreen);
            Random rng = new Random();
            var f = b.Db.MapDb.Features;
            while (true)
            {
                m.TeleportTo(f[rng.Next(f.Count)]);
                Console.WriteLine("done");
                var p = b.S<screens.PlayingScreen>();
                p.OpenMap();

            }
        }

    }
}

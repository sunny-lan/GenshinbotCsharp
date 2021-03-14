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
    class MapScreenDb
    {

        public class ResolutionDependent
        {
            public Point2d ActionBtnLoc { get; internal set; }
        }

        public Dictionary<Size, ResolutionDependent> R { get; set; } = new Dictionary<Size, ResolutionDependent>
        {
            [new Size(1440,900)]=new ResolutionDependent {
                ActionBtnLoc=new Point2d(1234,846)
            }
        };
    }
    class MapScreen : Screen
    {
        private GenshinBot b;
        private Screenshot.Buffer buf;
        private MapScreenDb db=new MapScreenDb();//TODO

        public Mat Map => buf.Mat;

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
            var r = b.W.GetRect();
            if (buf==null  || r.Size != buf.Size)
            {
                buf = Screenshot.GetBuffer(r.Width, r.Height);
            }

            b.W.TakeScreenshot(0, 0, buf);

            features = null;
            location = null;
        }

        public List<algorithm.MapTemplateMatch.Result> GetFeatures()
        {
            if (features == null)
                features = TemplateMatch.FindTeleporters(buf.Mat).ToList();
            return features;
        }

        public bool ExpectUnknown = true;

        public algorithm.MapLocationMatch.Result GetLocation()
        {
            if (location == null)
                location = LocationMatch.FindLocation2(GetFeatures(), b.W.GetRect().Size.cv(), ExpectUnknown);
            return location;
        }

        public void TeleportTo(Feature teleporter)
        {
            Debug.Assert(teleporter.Type == FeatureType.Teleporter);
            var p = ShowOnScreen(teleporter.Coordinates);
            b.W.MouseTo(p);
            b.W.MouseClick(0);
            Thread.Sleep(1000);
            b.W.MouseTo(db.R[b.W.GetSize()].ActionBtnLoc);
            b.W.MouseClick(0);
            Thread.Sleep(2000);//TODO
            b.S(b.PlayingScreen);
        }

        /// <summary>
        /// Returns the screen point of a coordinate. If the point is not on screen, it will try to move towards the point
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public Point2d ShowOnScreen(Point2d coord)
        {
            input.MouseMover m = new input.MouseMover(b.W);


            while (true)
            {
                UpdateScreenshot();
                var r = b.W.GetBounds().Cv();
                var l = GetLocation();
                var point = l.ToPoint(coord);

                if (r.Contains(point.ToPoint()))
                {
                    return point;
                }



                var center = r.Center();
                b.W.I.MouseTo(center);
                Thread.Sleep(10);
                b.W.K.KeyDown(input.GenshinKeys.Attack);
                Thread.Sleep(10);
                m.Goto((center - point).LimitDistance(200)+center).Wait();
                Thread.Sleep(100);
                b.W.K.KeyUp(input.GenshinKeys.Attack);

            }
        }

        public static void Test()
        {
            GenshinBot b = new GenshinBot();
            var m=b.S(b.MapScreen);
            int i = 0;
            while (true)
            {
                Console.ReadKey();
                m.TeleportTo(b.Db.MapDb.Features[i]);
                i++;
                var p = b.S<screens.PlayingScreen>();
                p.OpenMap();
            }
        }

    }
}

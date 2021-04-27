using genshinbot.screens;
using genshinbot.data.map;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Math;
using System.Diagnostics;
using genshinbot.data;
using genshinbot.automation.input;
using System.Reactive.Linq;
using genshinbot.util;
using System.Reactive;
using OneOf;

namespace genshinbot.screens
{

    using LocationResult = OneOf<
        algorithm.MapLocationMatch.Result,
        algorithm.MapLocationMatch.NoSolutionException>;
    public class MapScreen
    {
        public class Db
        {

            public class RD
            {
                public Point2d ActionBtnLoc { get; set; }
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

        private Db db = new Db();

        private BotIO b;

        algorithm.MapLocationMatch locationMatch;
        algorithm.MapTemplateMatch templateMatch;

        public IObservable<Mat> Screen { get; private init; }
        public IObservable<List<algorithm.MapTemplateMatch.Result>> Features { get; private init; }
        public IObservable<LocationResult> Location { get; private init; }

        public MapScreen(BotIO b)
        {
            this.b = b;

            locationMatch = new algorithm.MapLocationMatch(Data.MapDb.Features);
            templateMatch = new algorithm.MapTemplateMatch();

            Screen = b.W.Screen.Watch(b.W.Bounds);
            Features = Screen.Select(map => templateMatch.FindTeleporters(map).ToList());
            Location = Observable.CombineLatest(Features, b.W.Size, (features, size) =>
            {
                LocationResult res;
                try
                {
                    res = locationMatch.FindLocation2(features, size, ExpectUnknown);
                }
                catch (algorithm.MapLocationMatch.NoSolutionException e)
                {
                    res = e;
                }
                return res;
            });

        }

        public async Task Close()
        {
            await b.K.KeyPress(Keys.Escape);
            await Task.Delay(2000);//TODO
            //switch screen
        }

        public void AddFeature(Feature f)
        {
            Data.MapDb.Features.Add(f);
            locationMatch.AddFeature(f);
        }



        public bool ExpectUnknown = true;


        public async Task TeleportTo(Feature teleporter)
        {
            //TODO 
            throw new NotImplementedException();
            /*
            Debug.Assert(teleporter.Type == FeatureType.Teleporter);
            var p = ShowOnScreen(teleporter.Coordinates);
            await b.M.MouseTo(p);
            await b.M.MouseClick(0);
            await Task.Delay(1000);
            await b.M.MouseTo(db.R[b.W.Size.Get()].ActionBtnLoc);
            await b.M.MouseClick(0);
           //TODO b.SWait(b.LoadingScreen);
            //TODO b.LoadingScreen.WaitTillDone();
            Thread.Sleep(1000);
           // b.S(b.PlayingScreen);*/
        }

        /// <summary>
        /// Returns the screen point of a coordinate. If the point is not on screen, it will try to move towards the point
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public Point2d ShowOnScreen(Point2d coord)
        {//TODO 
            throw new NotImplementedException();
            /*
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

                b.W.MouseTo(beginPos);
                Thread.Sleep(10);

                b.W.MouseDown(0);
                Thread.Sleep(10);

                b.M.Goto((beginPos - point).LimitDistance(200)+ beginPos).Wait();
                Thread.Sleep(100); //pause to prevent flick gesture from happening

                b.W.MouseUp(0);
                Thread.Sleep(10);

            }
            */
        }

        /*  public static void Test()
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
                  var p = b.S<PlayingScreen>();
                  p.OpenMap();

              }
          }*/
        public static void Test2(ITestingRig rig1)
        {
            var rig = rig1.Make();
            var screen = new MapScreen(rig);
            using (screen.Location.Subscribe(x =>x.Switch(
                result=>Console.WriteLine($"t={result.Translation} s={result.Scale}"),
                error=>Console.WriteLine($"err={error}")
            )))
            {
                Console.ReadLine();
            }
        }

    }
}

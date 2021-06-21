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
using System.Reactive;
using OneOf;
using genshinbot.reactive;
using genshinbot.diag;
using genshinbot.reactive.wire;

namespace genshinbot.screens
{

    public class MapScreen : IScreen
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


        algorithm.MapLocationMatch locationMatch;
        algorithm.MapTemplateMatch templateMatch;

        public IObservable<Mat> Screen { get; private init; }
        public IObservable<List<algorithm.MapTemplateMatch.Result>> Features { get; private init; }
        public IObservable<algorithm.MapLocationMatch.Result> Screen2Coord { get; private init; }


        public MapScreen(BotIO b, ScreenManager screenManager) : base(b, screenManager)
        {

            locationMatch = new algorithm.MapLocationMatch(Data.MapDb.Features);
            templateMatch = new algorithm.MapTemplateMatch();

            Screen = b.W.Screen.Watch(b.W.Bounds).Depacket().AsObservable();//TODO
            Features = Screen.ProcessAsync(map =>
            {
                var k = templateMatch.FindTeleporters(map).ToList();
                Debug.Assert(k.Count > 0);//TODO
                return k;
            });
            Screen2Coord = Observable//TODO
                .CombineLatest(Features, b.W.Size.AsObservable(), (features, size) => (features, size))
                .ProcessAsync(x =>
                {
                    var (features, size) = x;
                    try
                    {
                        return locationMatch.FindLocation2(features, size, ExpectUnknown);
                    }
                    catch (algorithm.MapLocationMatch.NoSolutionException e)
                    {
                        Debug.Assert(false);//TODO
                        return null;
                    }
                })
                .NonNull();


        }

        public async Task Close()
        {
            await Io.K.KeyPress(Keys.Escape);
            await ScreenManager.ExpectScreen(ScreenManager.PlayingScreen);
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
        public static async Task Test2Async(ITestingRig rig1)
        {
            var rig = rig1.Make();
            var screen = new MapScreen(rig, null);
            Console.WriteLine("await get before");
            var loc = await screen.Screen2Coord.Get();
            Console.WriteLine("await get after");
            Console.ReadLine();
            using (screen.Screen2Coord.Subscribe(x => Console.WriteLine($"t={x.Translation} s={x.Scale}")))
            {
                Console.ReadLine();
            }

        }
        public static async Task Test3Async()
        {
            var gw = new MockGenshinWindow(new Size(1680, 1050));
            gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");
            gw.PlayingScreen.Image = Data.Imread("test/playing_luhua_1050.png");
            gw.CurrentScreen = gw.MapScreen;


            var rig1 = new MockTestingRig(gw);
            await Test2Async(rig1);
        }
    }
}

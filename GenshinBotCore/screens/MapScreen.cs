﻿using genshinbot.screens;
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
            public static DbInst<Db> Instance = new DbInst<Db>("screens/MapScreen.json");
            public class RD
            {
                public Point2d ActionBtnLoc { get; set; }
                public Size ActiveArea { get; set; }

                public Rect SelectorArea { get; set; }
                public Rect ActionBtnArea { get; set; }

                public Snap? TeleporterSnap { get; set; }
                public Snap? DomainSnap { get; set; }
                public Snap? Statue7Snap { get; set; }


                public Snap? TeleportButtonSnap { get; set; }

            }


            public Dictionary<Size, RD> R { get; set; } = new Dictionary<Size, RD>();
            public double FeatureSelectThres { get; set; } = 0.2;
            public double TeleportSnapThres { get; set; } = 3000000;
        }

        private Db db = Db.Instance.Value;


        algorithm.MapLocationMatch locationMatch;
        algorithm.MapTemplateMatch templateMatch;

        public IWire<Pkt<Mat>> Screen { get; }
        public IWire<Pkt<List<algorithm.MapTemplateMatch.Result>>> Features { get; }
        public IWire<Pkt<algorithm.MapLocationMatch.Result>> Screen2Coord { get; }

        public event Action<Exception> OnMatchError;

        public MapScreen(BotIO b, ScreenManager screenManager) : base(b, screenManager)
        {

            locationMatch = new algorithm.MapLocationMatch(Data.MapDb.Features
                .Where(x => x.Type == FeatureType.Teleporter).ToList());
            templateMatch = new algorithm.MapTemplateMatch();

            Screen = b.W.Screen.Watch2(b.W.Bounds);//TODO async
            Features = Screen.Select(
                (Mat map) =>
                {
                    var k = templateMatch.FindTeleporters(map).ToList();
                    if (k.Count == 0) throw new algorithm.AlgorithmFailedException("No teleporters found");
                    return k;
                }
            );
            Screen2Coord = b.W.Size.Select3((Size size) =>
                Features.Select(
                    features => locationMatch.FindLocation2(features, size, ExpectUnknown)
                )
            ).Switch2();
            this.b = b;
        }



        public async Task<(Rect? region, double score)> ScanForFeatureSelect(FeatureType f)
        {
            var sz = await b.W.Size.Value2();
            var rd = db.R[sz];
            Snap? s = f switch
            {
                FeatureType.Domain => rd.DomainSnap,
                FeatureType.Teleporter => rd.TeleporterSnap,
                FeatureType.Statue7 => rd.Statue7Snap,
                _ => throw new NotSupportedException()
            };

            var scanRegion = Util.RectAround(
                new Point(s!.Region.Left, rd.SelectorArea.Top),
                new Point(s!.Region.Right, rd.SelectorArea.Bottom)
            );

            var scan = await b.W.Size.Lock(sz, async () =>
            {
                return await b.W.Screen.Watch(scanRegion).Get();
            });

            //TODO use global
            using var match = scan.MatchTemplate(s.Image.Value, TemplateMatchModes.SqDiffNormed);
            match.MinMaxLoc(out var minVal, out var _, out var minLoc, out var _);
            if (minVal <= db.FeatureSelectThres)
                return (new Rect(minLoc + scanRegion.TopLeft, s.Region.Size), minVal);
            else return (null, minVal);
        }

        public async Task Close()
        {
            await Io.K.KeyPress(Keys.Escape);
            await ScreenManager.ExpectScreen(ScreenManager.PlayingScreen);
        }



        public bool ExpectUnknown = true;
        private readonly BotIO b;
        public async Task<(bool Open, double Score)> CheckIsTeleportButtonOpen()
        {

            var sz = await b.W.Size.Value2();
            var rd = db.R[sz];

            var btnImg = await b.W.Size.Lock(sz, b.W.Screen.Watch(rd.TeleportButtonSnap!.Region).Get);
            using var templateMatch = btnImg.MatchTemplate(rd.TeleportButtonSnap!.Image.Value, TemplateMatchModes.SqDiffNormed);
            templateMatch.MinMaxLoc(out double score, out var _);

            return (score <= db.FeatureSelectThres, score);
        }

        public async Task TeleportTo(Feature teleporter)
        {
            Debug.Assert(ScreenManager.ActiveScreen.Value == this);
            Debug.Assert(teleporter.Type.CanTeleport());
            var p = await ShowOnScreen(teleporter.Coordinates);
            await Io.M.MouseTo(p);
            await Io.M.MouseClick(0);
            await Task.Delay(1000);

            if ((await CheckIsTeleportButtonOpen()).Open is false)
            {
                var res = await ScanForFeatureSelect(teleporter.Type);
                await Io.M.MouseClick(MouseBtn.Left, res.region.Expect().RandomWithin());
                await Task.Delay(1000);
                Debug.Assert((await CheckIsTeleportButtonOpen()).Open);
            }


            await Io.M.MouseTo(db.R[await Io.W.Size.Value2()].ActionBtnLoc);
            await Io.M.MouseClick(0);
            //TODO b.SWait(b.LoadingScreen);
            //TODO b.LoadingScreen.WaitTillDone();
            //await Task.Delay(1000);
            await ScreenManager.ExpectScreen(ScreenManager.LoadingScreen);
            await ScreenManager.LoadingScreen.WaitTillDone();
            await ScreenManager.ExpectScreen(ScreenManager.PlayingScreen);
            // b.S(b.PlayingScreen);
        }

        /// <summary>
        /// Returns the screen point of a coordinate. If the point is not on screen, it will try to move towards the point
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public async Task<Point2d> ShowOnScreen(Point2d coord)
        {
            var mm = new input.MouseMover2(Io.W.Mouse);
            while (true)
            {
                var r = await Io.W.Bounds.Value2();
                var center = r.Center();
                var active = center.RectAround(db.R[r.Size].ActiveArea);

                var l = await Screen2Coord.Get();
                var point = l.ToPoint(coord);

                if (active.Contains(point.Round()))
                {
                    return point;
                }

                //use a randomized draw start location, as the mouse move doesn't work when
                // the drag begin location is on a clickable thing
                var beginPos = active.RandomWithin();

                await Io.M.MouseTo(beginPos);
                await Task.Delay(10);

                await Io.M.MouseDown(MouseBtn.Left);
                await Task.Delay(10);

                var dst = (beginPos - point).LimitDistance(200) + beginPos;
                Console.WriteLine($"goto {dst} point={point} begin={beginPos}");
                await mm.Goto(dst);
                await Task.Delay(100); //pause to prevent flick gesture from happening
                Console.WriteLine("dd");

                await Io.M.MouseUp(MouseBtn.Left);
                await Task.Delay(10);

            }

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
        public static async Task Testshow(ITestingRig rig1)
        {
            var rig = rig1.Make();
            var screen = new MapScreen(rig, null);
            await screen.Io.M.MouseTo(
                 await screen.ShowOnScreen(Data.MapDb.Features[0].Coordinates));

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

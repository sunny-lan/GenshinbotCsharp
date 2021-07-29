using genshinbot.screens;
using genshinbot.data;
using genshinbot.data.map;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using genshinbot.reactive;
using genshinbot.diag;
using genshinbot.reactive.wire;
using genshinbot.automation.input;
using genshinbot.util;

namespace genshinbot.controllers
{
    public class LocationManager
    {
        public readonly ScreenManager screens;


        public LocationManager(ScreenManager s)
        {

            this.screens = s;

            //if we don't know transformation, we can get it using knownpoints
            if (Data.MapDb.Coord2Minimap == null)
                CalculateCoord2Minimap();

            //TODO hack
            LastKnownPos.NonNull().Subscribe(x =>
            {

                var coord2Mini = Data.MapDb.Coord2Minimap.Expect();
                screens.PlayingScreen.approxPos =coord2Mini.Transform(x);
            });

        }




        public void CalculateCoord2Minimap()
        {
            var db = Data.MapDb;
            db.CalculateCoord2Minimap();
        }
        LiveWireSource<bool> _isTracking = new(false);
        public ILiveWire<bool> IsTracking => _isTracking;

        LiveWireSource<Point2d?> _lastKnownPos = new(null);
        public ILiveWire<Point2d?> LastKnownPos => _lastKnownPos;
        Task<IWire<Pkt<Point2d>>>? _trackMemo = null;
        object trackLock = new object();
        /// <summary>
        /// it is up to the user to call this in the correct timing! (aka no concurrent calls)
        /// </summary>
        /// <returns></returns>
        public Task<IWire<Pkt<Point2d>>> TrackPos()
        {

            async Task<IWire<Pkt<Point2d>>> meme_TrackPos()
            {
                try
                {
                    _isTracking.SetValue(true);
                    var db = Data.MapDb;
                    var coord2Mini = db.Coord2Minimap.Expect();
                    var res = await _TrackPos();
                    return res
                        .Select(x => coord2Mini.Inverse(x))
                        .Do(x => _lastKnownPos.SetValue(x))
                        .Catch(e =>
                        {
                            lock (trackLock)
                                _trackMemo = null;

                            _isTracking.SetValue(false);
                        });
                }
                catch
                {
                    lock (trackLock)
                        _trackMemo = null;

                    _isTracking.SetValue(false);
                    throw;
                }
            }

            lock (trackLock)
            {
                if (_trackMemo is null)
                {
                    _trackMemo = meme_TrackPos();
                }
                return _trackMemo;
            }


        }
        async Task<IWire<Pkt<Point2d>>> _TrackPos()
        {
            //  if (_memo is not null) return await _memo;
            //  TaskCompletionSource< IWire < Pkt < Point2d >>> sc = new ();

            var db = Data.MapDb;
            var coord2Mini = db.Coord2Minimap.Expect();

            var map = screens.MapScreen;
            var screen = screens.ActiveScreen.Value;
            if (screen != screens.PlayingScreen)
            {
                if (screen == map)
                {
                    //Console.WriteLine("track: map still open, closing");
                    var s2c = await map.Screen2Coord.Get();
                   // Console.WriteLine("track: map closed");
                    var center1 = (await map.Io.W.Bounds.Value2()).Center();
                    _lastKnownPos.SetValue(s2c.ToCoord(center1));
                    await screens.MapScreen.Close();
                }
                else
                {
                    throw new Exception("unexpected screen");
                }
            }
            else
            {
               // Console.WriteLine("track: playing screen open");
            }

            if (_lastKnownPos.Value is not null)
            {
                var res = screens.PlayingScreen.MinimapPos;
                try
                {
                    await res.Get();
                    return res;
                }
                catch (algorithm.AlgorithmFailedException)
                {
                    _lastKnownPos.SetValue(null);
                }
            }

            if (_lastKnownPos.Value is null)
            {
               // Console.WriteLine("track: lastknownpos null, opening map");
                await screens.PlayingScreen.OpenMap();
               // Console.WriteLine("track: map opened");
                var center = (await map.Io.W.Bounds.Value2()).Center();
                algorithm.MapLocationMatch.Result? screen2Coord;
                screen2Coord = await map.Screen2Coord.Get();
                _lastKnownPos.SetValue(screen2Coord.ToCoord(center));
            }

           // Console.WriteLine("track: closing map");
            await map.Close();
           // Console.WriteLine("track: map closed");
            return screens.PlayingScreen.MinimapPos;
        }

        public class WalkOptions
        {
            public bool ExpectClimb { get; init; } = false;
            public int WaitResponse { get; init; } = 600;
            public int DeadTrigger { get; init; } = 30;
            public int BackupTime { get; init; } = 1000;

            /// <summary>
            /// Set to null to always teleport
            /// </summary>
            public double? TeleportThres { get; init; } = 7;

            public int MaxArrowDetectFail { get; init; } = 4;
            public double AngleTolerance { get; internal set; } = Math.PI / 8;
        }
        public static WalkOptions DefaultWalkOptions = new WalkOptions();

        enum WalkStatus
        {
            Deading,
            Arrived,
            NotFlyingAnymore
        }

        public record WalkPoint(
            Point2d Value,
            double? Tolerance = null,
            bool ExpectClimb=false
        );

        public async Task WalkTo(List<WalkPoint> dst, WalkOptions? opt = null)
        {
            var opt2 = opt ?? DefaultWalkOptions;
            int idx = 0;
            var pos = await TrackPos();
            var wanted = new LiveWireSource<double?>(null);
            var errStream = new LiveWireSource<double?>(null);

            if (screens.ActiveScreen.Value != screens.PlayingScreen)
            {
                if (screens.ActiveScreen.Value == screens.MapScreen)
                {
                    await screens.MapScreen.Close();
                }
            }

            Debug.Assert(screens.ActiveScreen.Value == screens.PlayingScreen);
            PlayingScreen playingScreen = screens.PlayingScreen;
            //LiveWireSource<bool> wantedRelay = new LiveWireSource<bool>(false);
            var arrowControl = new algorithm.ArrowSteering(
                playingScreen.ArrowDirection,
                wanted//.Relay2(wantedRelay)
                      );

            var deltaP = arrowControl.MouseDelta.Select(x =>
               new Point2d(x, 0));
            var smoother = new MouseSmoother(deltaP);
            BotIO io = playingScreen.Io;
            // walking:
            //    Debug.WriteLine("enter status=walking");
            //assume we are initially walking
            /*  using (var allDead = playingScreen.IsAllDead
                      .Depacket()
                      .Where(x => x)
                      .Select(_ => WalkStatus.Deading)
                      .GetGetter()
                      )*/

            //switch to c4 (expected fischl)
           // await io.K.KeyPress(Keys.Oem4);
            //await playingScreen.PlayerSelect[3].Where(x => x).Get();
            int retryCount = 0;
            SemaphoreSlim bad = new SemaphoreSlim(1);
            using (deltaP.Subscribe(async delta =>
            {
                Interlocked.Exchange(ref retryCount, 0);
                await io.M.MouseMove(delta).ConfigureAwait(false);
            }, onErr: e=> { 
                if(e is algorithm.AlgorithmFailedException)
                {
                    if(Interlocked.Increment(ref retryCount) < opt2.MaxArrowDetectFail)
                    {
                        Console.WriteLine($"warn: arrow detect failed: {e}");
                        return;
                    }
                }
                errStream.EmitError(e);
            }))
            using (pos.Subscribe((Point2d p) =>
           {
               //  Console.WriteLine($"p={p}");
               /* if (p.DistanceTo(dst) < opt2.Tolerance)
                {
                    Debug.WriteLine("arrived quick kill hack");
                    await io.K.KeyUp(Keys.W);//TODO hak
                    wanted.SetValue(null);
                    return;
                }*/
               while (idx < dst.Count)
               {
                   double tol = dst[idx].Tolerance ?? (idx == dst.Count - 1 ? 4 : 2);
                   if (dst[idx].Value.DistanceTo(p) > tol) break;
                   idx++;
               }
               if (idx == dst.Count)
               {
                   errStream.SetValue(double.NaN);//send nan to represent done!
                   return;
               }
               wanted.SetValue(p.AngleTo(dst[idx].Value));
           }, onErr: E=>errStream.EmitError(E)))
           /*TODO make this work! using(Wire.CombineLatest(wanted.NonNull(), playingScreen.ArrowDirection.Depacket(), (double wanted, double known)=>
           {

               return (wanted-known)<opt2.AngleTolerance;
           }).DistinctUntilChanged().Subscribe(async (bool run)=>
           {
               await bad.WaitAsync();
               try
               {
                   //if angle is off utilize bow user
                   if (!run)
                   {
                       wantedRelay.SetValue(false);
                       await io.K.Key(Keys.W, run);
                       if (!(await playingScreen.BowActivated.Get()).matched)
                       {
                           await io.K.KeyPress(Keys.R);
                           await playingScreen.BowActivated.Where(x => x.matched).Get();
                       }
                       wantedRelay.SetValue(true);

                   }
                   else
                   {
                       if ((await playingScreen.BowActivated.Get()).matched)
                       {
                           await io.K.KeyPress(Keys.R);
                           await playingScreen.BowActivated.Where(x => !x.matched).Get();
                       }
                       await io.K.Key(Keys.W, run);
                       wantedRelay.SetValue(true);
                   }
               }
               finally
               {
                   bad.Release();
               }

           }))*/
            {  //keep going until either atDest, or dead

                //dont start till mouse ready
                await wanted.NonNull().Get().ConfigureAwait(false);
                
                try
                {
                    Debug.WriteLine("begin walking");
                    await io.K.KeyDown(Keys.W).ConfigureAwait(false);
                    await errStream.Where(x => double.IsNaN(x ?? 0)).Get();
                    return;
                    /*backtowalking:
                    var r = await await Task.WhenAny(allDead.Get(), atDest.Get()).ConfigureAwait(false);

                    Debug.WriteLine("walking ended");
                    if (r == WalkStatus.Arrived)
                    {
                        Debug.WriteLine("    arrived");

                        return;
                    }
                    else if (r == WalkStatus.Deading)
                    {
                        Debug.WriteLine("    deading - check to make sure dead (first press space)");
                        await Task.Delay(350).ConfigureAwait(false);
                        await io.K.KeyPress(Keys.Space).ConfigureAwait(false);
                        //check again to make sure
                        await Task.Delay(opt2.WaitResponse).ConfigureAwait(false);
                        if (await playingScreen.IsAllDead.Get().ConfigureAwait(false))
                        {

                            Debug.WriteLine("       yup");
                            goto dead;
                        }
                        else
                        {
                            Debug.WriteLine("       nope - back to walking");
                            goto backtowalking;
                        }
                    }*/
                }
                finally
                {
                    await io.K.KeyUp(Keys.W).ConfigureAwait(false);
                    // wanted.SetValue(null);
                }

            }
            /*
        dead:
            Debug.WriteLine("enter status=dead");

            //todo sometimes climbing may be valid
            if (await playingScreen.IsClimbing.Get().ConfigureAwait(false))
            {
                Debug.WriteLine("   detect climbing");
                //keep pressing x till we're on ground
                do
                {
                    Debug.WriteLine("       try drop");
                    await io.K.KeyPress(Keys.X).ConfigureAwait(false);
                    await Task.Delay(opt2.WaitResponse).ConfigureAwait(false);
                } while (!await playingScreen.IsAllDead.Get().ConfigureAwait(false));

                Debug.WriteLine("       climb sucessfully cancelled. back up");
                try
                {
                    await io.K.KeyDown(Keys.S).ConfigureAwait(false);
                    await Task.Delay(opt2.BackupTime).ConfigureAwait(false);
                }
                finally
                {
                    await io.K.KeyUp(Keys.S).ConfigureAwait(false);

                }
                goto walking;
            }

        todo
         * rightnow impoosible to fly here
        var isFly = await playingScreen.IsFlying.Get();
        if (isFly) goto flying;
        else
        {

        falling:
            Debug.WriteLine("enter status=falling");
            //keep jumping until either we are flying, or walking
            do
            {
                if (await playingScreen.IsFlying.Get().ConfigureAwait(false))
                {
                    Debug.WriteLine("   successfully enter flying state");
                    goto flying;
                }

                if (!await playingScreen.IsAllDead.Get().ConfigureAwait(false))
                {
                    Debug.WriteLine("   landed after falling");
                    goto walking;
                }

                Debug.WriteLine("   press space");
                await io.K.KeyPress(Keys.Space).ConfigureAwait(false);
                await Task.Delay(opt2.WaitResponse).ConfigureAwait(false);

               
                //todo we may be climbing
            } while (true);


        flying:
            Debug.WriteLine("enter status=flying");
            using (deltaP.Subscribe(async delta =>
            {
                Console.WriteLine($"fly d={delta}");
                await io.M.MouseMove(delta).ConfigureAwait(false);
            }))
            using (pos.Subscribe(p =>
            {
                Console.WriteLine($"fly p={p}");
                wanted.SetValue(p.AngleTo(dst));
            }))
            {
                //dont start till mouse ready
                await wanted.NonNull().Get().ConfigureAwait(false);
                try
                {
                    Debug.WriteLine("   begin moving");
                    await io.K.KeyDown(Keys.W).ConfigureAwait(false);

                    //keep going until not flying, or at dest
                    using (var notFlying = playingScreen.IsFlying
                        .Depacket()
                        .Where(x => !x)
                        .Select(_ => WalkStatus.NotFlyingAnymore)
                        .GetGetter()
                        )
                    using (var atDest = pos
                        .Depacket()
                        .Where(p => p.DistanceTo(dst) < opt2.Tolerance)
                        .Select(_ => WalkStatus.Arrived)
                        .GetGetter()
                    )
                    {
                        var r = await await Task.WhenAny(notFlying.Get(), atDest.Get()).ConfigureAwait(false);
                        if (r == WalkStatus.Arrived)
                        {
                            Debug.WriteLine("      arrived while flying");
                            return;
                        }
                        else if (r == WalkStatus.NotFlyingAnymore)
                        {
                            Debug.WriteLine("      not flying anymore. enter next state");
                            // we may have landed or climbing now
                            //or ran out of stamina 
                            //todo running out of stamina is not handled 
                            if (await playingScreen.IsAllDead.Get().ConfigureAwait(false))
                                goto dead;
                            else
                                goto walking;
                        }
                    }
                }
                finally
                {
                    await io.K.KeyUp(Keys.W).ConfigureAwait(false);
                    wanted.SetValue(null);
                }
            }*/
            Debug.Assert(false, "invalid state");

        }

        public async Task TeleportTo(Feature waypoint)
        {
            Debug.Assert(waypoint.Type.CanTeleport());
            var m = screens.MapScreen;
            if (screens.ActiveScreen.Value != m)
            {
                if (screens.ActiveScreen.Value == screens.PlayingScreen)
                    await screens.PlayingScreen.OpenMap();
                else
                    throw new Exception("expected playing or map screen");
            }
            await m.TeleportTo(waypoint);
            _lastKnownPos.SetValue(waypoint.Coordinates);

        }
        /* 
                  public Point2d DeduceLocation()
                 {
                     var db = this.Data.MapDb;
                     if (db.Coord2Minimap == null)
                         throw new Exception("Missing setting");
                     bool approxLocCalculated = false;

                     var p = b.S<PlayingScreen>();
                 begin:

                     //check map to find initial location
                     if (this.approxPos == null)
                     {
                         approxPos = GetLocationFromMap();

                         approxLocCalculated = true;
                     }
                     Mat minimap = p.SnapMinimap();



                     Point2d miniPos1;
                     if (pt == null) //if the scale hasn't been calculated yet
                     {
                         Console.WriteLine("Recalculating scale");
                         //convert map coord to minimap pos so we can use minimap matcher
                         var bigApproxPos = db.Coord2Minimap.Expect().Transform(this.approxPos.Expect());
                         var miniPos = m.FindScale(bigApproxPos, minimap, out var pt1);

                         //we were unable to find a valid scale
                         if (miniPos == null)
                         {
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

                         //we successfully found the coordinate
                         miniPos1 = miniPos.Expect();
                         pt = new algorithm.MinimapMatch.PositionTracker(pt1);
                     }
                     else
                     {
                         //if we found the scale already, we can continue tracking
                         var miniPos = pt.Track(minimap);

                         if (miniPos == null)
                         {
                             //if tracking failed we need to invalidate the scale
                             // and try again
                             pt = null;
                             goto begin;
                         }

                         miniPos1 = miniPos.Expect();
                     }

                     //convert minimap point back to map coordinates
                     var coord = db.Coord2Minimap.Expect().Inverse(miniPos1);

                     //also, we can update approxPos in case we need it later
                     approxPos = coord;

                     return coord;
                 }

               public void WalkTo(Point2d dstPos, double accuracy = 1)
                 {
                     var curPos = DeduceLocation();
                     var p = b.S<PlayingScreen>();



                     while (true)
                     {
                         if (curPos.DistanceTo(dstPos) <= accuracy)
                         {
                             b.K.KeyUp(input.GenshinKeys.Forward);
                             b.M.Stop();
                             return;
                         }

                         while (p.IsDisabled())
                         {
                             b.W.KeyPress((int)automation.input.Keys.Space);
                             Thread.Sleep(100);
                         }

                         b.K.KeyPress(input.GenshinKeys.Forward);
                         var dstAng = curPos.AngleTo(dstPos);
                         var curAng = p.GetArrowDirection();

                         var diff = curAng.RelativeAngle(dstAng);
                         b.M.Move(new Point2d(diff / 2, 0));
                         b.K.KeyDown(input.GenshinKeys.Forward);
                         curPos = DeduceLocation();

                     }
                 }

                 public static void Testwalkto()
                 {
                     Point2d dst = new Point2d(x: 1956.43237304688, y: -303.038940429688);
                     GenshinBot b = new GenshinBot();

                     // b.LocationManager.Coord2MinimapTool();

                     b.S(b.PlayingScreen);

                     b.LocationManager.WalkTo(dst);
                 }

                 public static void Test()
                 {
                     GenshinBot b = new GenshinBot();

                     // b.LocationManager.Coord2MinimapTool();

                     b.S(b.PlayingScreen);


                     while (true)
                     {
                         var pos = b.LocationManager.DeduceLocation();
                         Console.WriteLine(pos);
                     }
                 }



                 public void Coord2MinimapTool()
                 {
                     var data = new List<Tuple<Point2d, Point2d>>();
                     while (true)
                     {
                         Console.WriteLine("Please enter approx pos");
                         var approxPos = Util.ReadPoint();

                         Console.WriteLine("Please go into map screen");
                         Console.ReadLine();

                         //bot control begins

                         var m = b.S(b.MapScreen);
                         var coord1 = GetLocationFromMap();
                         m.Close();

                         var p = b.S<PlayingScreen>();
                         var mini = p.SnapMinimap();

                         var miniP1 = this.m.FindScale(approxPos, mini, out var _);
                         if (miniP1 == null)
                         {
                             Console.WriteLine("Minimap loc failed");
                             continue;
                         }

                         //bot control ends

                         Console.WriteLine("Data point #" + data.Count);
                         Console.WriteLine("  Map coordinate " + coord1);
                         Console.WriteLine("  Minimap pos " + miniP1);

                         data.Add(new Tuple<Point2d, Point2d>(coord1, miniP1.Expect()));

                         if (data.Count >= 2)
                         {

                         }

                         Console.WriteLine("Please move to a different point and open map");
                         Console.ReadKey();

                     }
                 }*/
        public record WholeWalk(
            Feature Teleporter,
            List<WalkPoint> Points
        );

        public async Task Goto(Feature dst, WalkOptions? opt = null)
        {
            var opt2 = opt ?? DefaultWalkOptions;
            var db = MapDb.Instance.Value;

            var feats = db.Features.Where(f => f.Type.CanTeleport());
            if (opt2.TeleportThres is double dd)
            {
                var kust = await TrackPos();//todo use approx pos only
                var pp = await kust.Get();
                feats = feats.Union(db.Features.Where(f =>
                    f.Coordinates.DistanceTo(pp) <= opt2.TeleportThres));
            }


            double bestLen = double.MaxValue;
            IEnumerable<Feature>? best = null;
            foreach (var src in feats)
            {
                var path = db.FindPath(src.ID, dst.ID);
                if (path is not null)
                {
                    var newLen = MapDb.Length(path);
                    if (newLen < bestLen)
                    {
                        best = path;
                        bestLen = newLen;
                    }
                }
            }

            if (best is null)
                throw new algorithm.AlgorithmFailedException("no path found");

            if (best.First().Type.CanTeleport())
            {
                await TeleportTo(best.First());
                best = best.Skip(1);
            }

            var lst = best
                    .Select(f => new WalkPoint(f.Coordinates))
                    .ToList();

            if (lst.Count > 0)
                await WalkTo(lst, opt);


        }

        public async Task WholeWalkTo(WholeWalk w, WalkOptions? opt = null)
        {
            await TeleportTo(w.Teleporter);
            await WalkTo(w.Points, opt);
        }

        public static async Task TestTrackAsync(ITestingRig rig)
        {
            BotIO b = rig.Make();
            ScreenManager mgr = new ScreenManager(b);
            mgr.ForceScreen(mgr.PlayingScreen);
            LocationManager lm = new LocationManager(mgr);
            var trackin = await lm.TrackPos();
            Console.WriteLine("trakin begin");

            using (trackin.Subscribe(
               x => Console.WriteLine(x)//,
                                        //   onError:x=>Console.WriteLine(x)
            ))
            /* using(mgr.PlayingScreen.ArrowDirection.Subscribe(

                 x => Console.WriteLine(x)
                 ))*/
            {
                Console.ReadLine();
            }
        }

        public static async Task TestGoto(ITestingRig rig)
        {
            //Point2d dst = new Point2d(x: 1956.43237304688, y: -303.038940429688);
            Point2d dst = new Point2d(x: 2207.3157080477517, y: -595.0791251572828);
            BotIO b = rig.Make();
            ScreenManager mgr = new ScreenManager(b);
            mgr.ForceScreen(mgr.PlayingScreen);
            LocationManager lm = new LocationManager(mgr);
            List<WalkPoint> bad = new();
            bad.Add(new WalkPoint(dst));
            while (true)
            {
                await lm.WalkTo(bad);
                Console.ReadLine();
            }
        }

        public class Test
        {
            private readonly LocationManager lm;

            public Test(LocationManager lm)
            {
                this.lm = lm;
            }

            public async Task TestGoto()
            {
                await lm.Goto(MapDb.Instance.
                    Value.Features.Find(x => x.ID == 74)!);
            }
        }
    }
}

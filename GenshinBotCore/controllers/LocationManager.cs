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

namespace genshinbot.controllers
{
    public class LocationManager
    {
        private ScreenManager screens;


        public LocationManager(ScreenManager s)
        {

            this.screens = s;

            //if we don't know transformation, we can get it using knownpoints
            if (Data.MapDb.Coord2Minimap == null)
                CalculateCoord2Minimap();

        

        }



        
        public void CalculateCoord2Minimap()
        {
            var db = Data.MapDb;
            Debug.Assert(db.KnownMinimapCoords.Count >= 2, "At least 2 points required");
            var a = db.KnownMinimapCoords[0];
            var b = db.KnownMinimapCoords[1];
            var deltaCoord = a.Coord - b.Coord;
            var deltaMini = a.Minimap - b.Minimap;
            double scaleX = deltaMini.X / deltaCoord.X;
            double scaleY = deltaMini.Y / deltaCoord.Y;
            Debug.Assert(Math.Abs(scaleY - scaleX) < db.MaxMinimapScaleDistortion, "Calculated scaling is non uniform");
            double scale = (scaleX + scaleY) / 2.0;
            db.Coord2Minimap = new data.Transformation
            {
                Scale = scale,
                Translation = a.Minimap - a.Coord * scale
            };
        }

        public async Task<IWire<Point2d>> TrackPos()
        {
            var db = Data.MapDb;
            var coord2Mini = db.Coord2Minimap.Expect();

            var map = screens.MapScreen;
            var screen=await screens.ActiveScreen.Get();
            if (screen != screens.PlayingScreen)
            {
                if(screen == map)
                {
                    await screens.MapScreen.Close();
                }
                else
                {
                    throw new Exception("unexpected screen");
                }
            }
            await screens.PlayingScreen.OpenMap();
            var center = (await map.Io.W.Bounds.Value2()).Center();
            
            var screen2Coord = await map.Screen2Coord.Get();
            var miniLoc = coord2Mini.Transform(screen2Coord.ToCoord(center));
            await map.Close();
            return screens.PlayingScreen.TrackPos(miniLoc)
                .Select(x => coord2Mini.Inverse(x));
        }


        /*  public void TeleportTo(Feature waypoint)
         {
             Debug.Assert(waypoint.Type == FeatureType.Teleporter);
             var m = b.MapScreen; //b.S<screens.MapScreen>();
             m.TeleportTo(waypoint);
             approxPos = waypoint.Coordinates;
             pt = null;
         }

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


        public static async Task testAsync(ITestingRig rig)
        {
            BotIO b = rig.Make();
            ScreenManager mgr = new ScreenManager(b);
            mgr.ForceScreen(mgr.PlayingScreen);
            LocationManager lm = new LocationManager(mgr);
            var trackin= await lm.TrackPos();
            Console.WriteLine("trakin begin");

            using (trackin.Subscribe(
               x=>Console.WriteLine(x)//,
             //   onError:x=>Console.WriteLine(x)
            ))
            using(mgr.PlayingScreen.ArrowDirection.Subscribe(

                x => Console.WriteLine(x)
                ))
            {
                Console.ReadLine();
            }
        }

        public static async Task testAsync3()
        {
            var gw = new MockGenshinWindow(new Size(1680, 1050));
            gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");
            gw.PlayingScreen.Image = Data.Imread("test/playing_luhua_1050.png");
            gw.CurrentScreen = gw.PlayingScreen;

            var rig1 = new MockTestingRig(gw);
            await testAsync(rig1);
        }
    }
}

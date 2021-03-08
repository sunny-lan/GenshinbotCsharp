using GenshinbotCsharp.database.controllers;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.controllers
{
    class LocationManager
    {
        GenshinBot b;
        LocationManagerDb db;

        public LocationManager(GenshinBot b)
        {
            this.b = b;
            this.db = b.Db.LocationManagerDb;

            this.m = new algorithm.MinimapMatch.ScaleMatcher(new algorithm.MinimapMatch.Settings
            {
                BigMap = b.Db.MapDb.BigMap.Load(),
            });

            //if we don't know transformation, we can get it using knownpoints
            if (db.Coord2Minimap == null)
                CalculateCoord2Minimap();
        }

        Point2d? approxPos;
        algorithm.MinimapMatch.ScaleMatcher m;
        algorithm.MinimapMatch.PositionTracker pt;

        public Point2d GetLocationFromMap()
        {
            var p = b.S<screens.PlayingScreen>();
            p.OpenMap();

            var m = b.S<screens.MapScreen>();

            m.UpdateScreenshot();
            var screenCenter = b.W.GetBounds().Cv().Center();
            var approxPos = m.GetLocation().ToCoord(screenCenter);

            m.Close();

            return approxPos;
        }

        public void CalculateCoord2Minimap()
        {
            Debug.Assert(db.KnownMinimapCoords.Count >= 2, "At least 2 points required");
            var a = db.KnownMinimapCoords[0];
            var b = db.KnownMinimapCoords[1];
            var deltaCoord = a.Coord - b.Coord;
            var deltaMini = a.Minimap - b.Minimap;
            double scaleX = deltaMini.X / deltaCoord.X;
            double scaleY = deltaMini.Y / deltaCoord.Y;
            Debug.Assert(Math.Abs(scaleY - scaleX) < db.MaxMinimapScaleDistortion, "Calculated scaling is non uniform");
            double scale = (scaleX + scaleY) / 2.0;
            db.Coord2Minimap = new database.Transformation
            {
                Scale = scale,
                Translation = a.Minimap - a.Coord * scale
            };
        }

        public Point2d DeduceLocation()
        {
            if (db.Coord2Minimap == null)
                throw new Exception("Missing setting");

            var p = b.S<screens.PlayingScreen>();

            Mat minimap = p.SnapMinimap();
            bool approxLocCalculated = false;

        begin:
            //TODO support starting from MapScreen

            //check map to find initial location
            if (this.approxPos == null)
            {
                approxPos = GetLocationFromMap();

                approxLocCalculated = true;
            }



            Point2d miniPos1;
            if (pt == null) //if the scale hasn't been calculated yet
            {
                Console.WriteLine("Recalculating scale");
                //convert map coord to minimap pos so we can use minimap matcher
                var bigApproxPos = db.Coord2Minimap.Transform(this.approxPos.Expect());
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
            var coord = db.Coord2Minimap.Inverse(miniPos1);

            //also, we can update approxPos in case we need it later
            approxPos = coord;

            return coord;
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

                var p = b.S<screens.PlayingScreen>();
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
        }
    }
}

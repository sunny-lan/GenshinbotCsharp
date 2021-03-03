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
                BigMap = Data.Imread("map/genshiniodata/assets/MapExtracted_12.png"),
            });
        }

        Point2d? approxPos;
        algorithm.MinimapMatch.ScaleMatcher m;
        algorithm.MinimapMatch.PositionTracker pt;

        public Point2d DeduceLocation(screens.PlayingScreen p)
        {
            Mat minimap = p.SnapMinimap();
            bool approxLocCalculated = false;

        begin:
            //TODO support starting from MapScreen

            //check map to find initial location
            if (this.approxPos == null)
            {
                var m = p.OpenMap();

                m.UpdateScreenshot();
                var screenCenter = b.W.GetBounds().Cv().Center();
                this.approxPos = m.GetLocation().ToCoord(screenCenter);

                m.Close();

                approxLocCalculated = true;
            }



            Point2d miniPos1;
            if (pt == null) //if the scale hasn't been calculated yet
            {
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
                miniPos1=miniPos.Expect();
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
            var coord =  db.Coord2Minimap.Inverse(miniPos1);

            //also, we can update approxPos in case we need it later
            approxPos = coord;

            return coord;
        }

        ~LocationManager()
        {
        }
    }
}

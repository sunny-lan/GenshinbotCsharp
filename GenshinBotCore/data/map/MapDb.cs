using genshinbot.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data.map
{
    public class MapDb
    {
        public static readonly DbInst<MapDb> Instance = new("map/db.json");
        public List<Feature> Features { get; set; } = new();

        public Feature? DeleteFeature(int id)
        {
            Feature? del = null;
            Feature? parent = null;
            foreach (var f in Features)
            {
                if (f.Reachable is not null)
                {
                    int oldlen = f.Reachable.Count;
                    f.Reachable = f.Reachable.Where(x => x != id).ToList();
                    if (oldlen != f.Reachable.Count)
                        parent = f;
                }

                if (f.ID == id)
                    del = f;
            }
            if (del is not null)
                Features.Remove(del);
            return parent;

        }
        public static double Length(List<Feature> path)
        {
            double res=0;
            for (int i = 0; i + 1 < path.Count; i++)
                res += path[i].Coordinates.DistanceTo(path[i + 1].Coordinates);
            return res;
        }
        public List<Feature>? FindPath(int src, int dst)
        {
            Dictionary<int, Feature> mapping = new();
            foreach (var f in Features)
                mapping[f.ID] = f;

            List<Feature> res = new();
            Dictionary<int, bool> visited = new();
            bool dfs(int i)
            {
                if (visited.GetValueOrDefault(i)) return false;
                visited[i] = true;

                var f = mapping[i]!;
                if (i == dst)
                {
                    res.Add(f);
                    return true;
                }
                if (f.Reachable is null) return false;

                res.Add(f);
                foreach (var child in f.Reachable!)
                {
                    if (dfs(child)) return true;
                }
                res.RemoveAt(res.Count - 1);
                return false;


            }

            if (dfs(src))

                return res;
            else return null;
        }

        public Image BigMap { get; set; } = new Image
        {
            Path = "map/genshiniodata/assets/MapExtracted_12.png",
        };


        /// <summary>
        /// Approximate transformation of a coordinate to a pixel on BigMap
        /// </summary>
        public Transformation? Coord2Minimap { get; set; }

        public void CalculateCoord2Minimap()
        {
            var db = this;
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

        /// <summary>
        /// Stores a list of known points on the minimap, 
        /// and the corresponding map coordinate
        /// Used to calculate coord2minimap
        /// Tuple.First=  minimap, second=coord
        /// </summary>
        public List<KnownPoint> KnownMinimapCoords { get; set; } = new List<KnownPoint>
            {
                new KnownPoint
                {
                    Minimap=new Point2d(x:3820.34275379058, y:1832.533690062),
                    Coord=new Point2d(x:2059.64044189453 ,y:-621.944061279297),
                },

                 new KnownPoint
                {
                    Minimap=new Point2d(x:2743.18303450733, y:3222.58239108457),
                    Coord=new Point2d(x:1093.4270324707, y:621.195953369141),
                }
            };
        public double MaxMinimapScaleDistortion { get; set; } = 0.005;

        public struct KnownPoint
        {
            public Point2d Minimap { get; set; }
            public Point2d Coord { get; set; }
        }
    }
}

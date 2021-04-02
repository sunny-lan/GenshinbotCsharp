using GenshinbotCsharp.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.database.map
{
    class MapDb
    {
        public List<Feature> Features { get; set; }


        public static MapDb Default() {
            return new MapDb
            {
                Features = new List<Feature>(),
            };
        }
        public Image BigMap { get; set; } = new Image
        {
            Path = "map/genshiniodata/assets/MapExtracted_12.png",
        };


        /// <summary>
        /// Approximate transformation of a coordinate to a pixel on BigMap
        /// </summary>
        public Transformation? Coord2Minimap { get; set; }

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
        public double MaxMinimapScaleDistortion { get; internal set; } = 0.005;

        public struct KnownPoint
        {
            public Point2d Minimap { get; set; }
            public Point2d Coord { get; set; }
        }
    }
}

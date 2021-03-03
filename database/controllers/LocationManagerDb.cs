using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.database.controllers
{
    class LocationManagerDb
    {
        public static LocationManagerDb Default()
        {
            return new LocationManagerDb { };
        }


        /// <summary>
        /// Approximate transformation of a coordinate to a pixel on BigMap
        /// </summary>
        public Transformation Coord2Minimap { get; set; }
    }
}

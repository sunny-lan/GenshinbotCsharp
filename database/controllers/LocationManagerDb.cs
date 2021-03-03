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


        public Image BigMap { get; set; }

        /// <summary>
        /// Approximate transformation of a coordinate to a pixel on BigMap
        /// </summary>
        public Transformation Coord2Minimap { get; set; }


        //used to bruteforce scale
        public double MinScale { get; internal set; } =1;
        public double MaxScale { get; internal set; } = 3;
        public double ScaleStep { get; internal set; } = 1.5;
        public int PlayerArrowRadius { get; internal set; }
        public int BigPadding { get; internal set; }

        //
    }
}

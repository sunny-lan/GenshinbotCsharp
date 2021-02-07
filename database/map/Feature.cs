
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.database.map
{
    enum FeatureType
    {
        Teleporter
    }
    class Feature
    {
        public FeatureType Type { get; set; }
        public Point2d Coordinates { get; set; }
    }
}

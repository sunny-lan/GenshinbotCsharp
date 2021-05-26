using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data
{
    public class ClickDb
    {
        public class PointSet
        {
            public Dictionary<Size2d, Point2d> RD { get; set; } = new Dictionary<Size2d, Point2d>();
        }

        public Dictionary<string, PointSet> RD { get; set; } = new Dictionary<string, PointSet>();
        
        public PointSet Get(string name)
        {
            return RD[name];
        }
    }

}

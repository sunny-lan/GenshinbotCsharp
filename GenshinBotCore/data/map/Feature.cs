
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data.map
{
    public enum FeatureType
    {
        Teleporter
    }
    public class Feature
    {
        private static int global_ctr = 0;
        public Feature()
        {
            _id = global_ctr;
            global_ctr++;
        }

        private int _id;
        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                //Sketchy way to make sure IDs don't repeat
                global_ctr = Math.Max(global_ctr, _id)+1;
            }
        }
        public FeatureType Type { get; set; }
        public Point2d Coordinates { get; set; }
    }
}

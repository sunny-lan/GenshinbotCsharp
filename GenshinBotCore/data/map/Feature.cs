
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
        Teleporter,
        RandomPoint,
    }
    public class Feature
    {
        private static object lck = new object();
        private static int global_ctr = 0;

        private int _id=-1;
        public int ID
        {
            get
            {
                if (_id == -1)
                {
                    lock (lck)
                    {
                        _id = global_ctr;
                        global_ctr++;
                    }
                }
                return _id;
            }

            set
            {
                _id = value;
                //Sketchy way to make sure IDs don't repeat
                lock (lck)
                    global_ctr = Math.Max(global_ctr, _id) + 1;
            }
        }
        public FeatureType Type { get; set; }
        public string? Name { get; set; }
        public Point2d Coordinates { get; set; }

        public List<int>? Reachable { get; set; }

    }
}

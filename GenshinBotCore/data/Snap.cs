using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data
{
    /// <summary>
    /// Represents a snapshot of a specific screen region to be used as reference
    /// </summary>
    public class Snap
    {
        public Mat Image { get; set; }
        public Rect Region { get; set; }
    }
}

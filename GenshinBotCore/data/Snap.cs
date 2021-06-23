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
        public SavableMat Image { get; set; }
        public Rect Region { get; set; }

        private Mat tmp = new Mat();
        public double Compare(Mat screen)
        {
            Cv2.Absdiff(screen, Image.Value,tmp);
            Cv2.Multiply(tmp, tmp, tmp);
            return Cv2.Sum(tmp).SumComponents();
        }

         ~Snap()
        {
            tmp.Dispose();
        }
    }
}

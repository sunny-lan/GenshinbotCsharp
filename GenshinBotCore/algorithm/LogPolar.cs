using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    public class LogPolar
    {
        public Mat output = new Mat();
        public double radius;

        ~LogPolar()
        {
            output.Dispose();
        }
        public void Run(Mat recomb, Size sz = default)
        {
            var center = recomb.Center();
            radius = Math.Min(center.X, center.Y);
            Cv2.WarpPolar(recomb, output, sz, center.ToPointf(), radius, InterpolationFlags.Linear, WarpPolarMode.Log);
        }

    }
}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot.database
{
    public struct ColorRange
    {
        public ColorRange(Scalar min, Scalar max)
        {
            Min = min;
            Max = max;
        }

        public Scalar Min { get; set; }
        public Scalar Max { get; set; }
    }

    public static class ColorRangeExt
    {
        public static Mat InRange(this Mat m, ColorRange r)
        {
            return m.InRange(r.Min, r.Max);
        }
    }
}

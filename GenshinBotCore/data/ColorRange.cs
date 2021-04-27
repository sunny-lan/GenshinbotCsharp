using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot.data
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

        public bool Contains(Scalar color)
        {
            return Min.LessEqual(color) && color.LessEqual(Max);
        }
    }

    public static class ColorRangeExt
    {
        public static bool Less(this Scalar s, Scalar b)
        {
            return s.Val0 < b.Val0 &&
                s.Val1 < b.Val1 &&
                s.Val2 < b.Val2 && 
                s.Val3 < b.Val3;
        }
        public static bool LessEqual(this Scalar s, Scalar b)
        {
            return s.Val0 <= b.Val0 &&
                s.Val1 <= b.Val1 &&
                s.Val2 <= b.Val2 &&
                s.Val3 <= b.Val3;
        }
        public static Mat InRange(this Mat m, ColorRange r)
        {
            return m.InRange(r.Min, r.Max);
        }
    }
}

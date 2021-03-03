using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.database
{
    class Transformation
    {
        public double Scale { get; set; }
        public Point2d Translation { get; set; }

        public Point2d Transform(Point2d p)
        {
            return  p*Scale + Translation;
        }

        public Point2d Inverse(Point2d p)
        {
            return (p - Translation) *(1/Scale);
        }
    }
}

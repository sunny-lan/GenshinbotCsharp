using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.data
{
    public class Transformation
    {
        /// <summary>
        /// Applied before translation
        /// </summary>
        public double Scale { get; set; } = 1;

        /// <summary>
        /// Applied after scale
        /// </summary>
        public Point2d Translation { get; set; } = Util.Origin;

        public Point2d Transform(Point2d p)
        {
            return p * Scale + Translation;
        }

        public Point2d Inverse(Point2d p)
        {
            return (p - Translation) * (1 / Scale);
        }
    }
}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.data
{
    public struct Transformation
    {
        /// <summary>
        /// Applied before translation
        /// </summary>
        public double Scale { get; set; } 

        /// <summary>
        /// Applied after scale
        /// </summary>
        public Point2d Translation { get; set; }

        public Transformation(double scale, Point2d translation)
        {
            Scale = scale;
            Translation = translation;
        }

        public static Transformation Unit()
        {
            return new Transformation(1, Util.Origin);
        }

        public Point2d Transform(Point2d p)
        {
            return p * Scale + Translation;
        }

        public Point2d Inverse(Point2d p)
        {
            return (p - Translation) * (1 / Scale);
        }

        public Rect2d Transform(Rect2d r)
        {
            return new Rect2d(Transform(r.Location), r.Size.Scale(Scale));
        }

        /// <summary>
        /// Find t' such that t'.Scale=scale and t'.Transform(center)=t.Transform(center)
        /// </summary>
        /// <param name="center"></param>
        /// <param name="scale"></param>
        /// <returns>t'</returns>
        public Transformation ScaleAround(Point2d center,double scale)
        {
            var origPt = Inverse(center);
            return new Transformation(scale, center - origPt * scale);
        }

        /// <summary>
        /// Find t' such that t'.scale=t.scale
        /// and t'.Transform(original)=transformed
        /// </summary>
        /// <param name="original"></param>
        /// <param name="transformed"></param>
        /// <returns>t'</returns>
        public Transformation MatchPoints(Point2d original,Point2d transformed)
        {
            return new Transformation(Scale, transformed - original * Scale);
        }
    }
}

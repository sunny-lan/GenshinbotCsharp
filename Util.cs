using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    static class Util
    {

        public static Point ToPoint(this Point2d p)
        {
            return new Point(p.X, p.Y);
        }
    }
}

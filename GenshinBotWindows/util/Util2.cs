using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot
{
    static class Util2
    {
        
        public static Rect Cv(this Vanara.PInvoke.RECT r)
        {
            return new Rect(r.X, r.Y, r.Width, r.Height);
        }

    }
}

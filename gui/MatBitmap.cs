using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.gui
{
    class MatBitmap
    {
        public static Bitmap From(Mat m)
        {
            return new Bitmap(m.Width,m.Height,
                (int)m.Step(),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                m.Data)
                ;
        }
    }
}

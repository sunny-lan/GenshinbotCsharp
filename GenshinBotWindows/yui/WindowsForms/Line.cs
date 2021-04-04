using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.WindowsForms
{
    class Line : Drawable, yui.Line
    {
        private Viewport parent;

        public Line(Viewport parent)
        {
            this.parent = parent;
        }

        OpenCvSharp.Point a, b;
        public OpenCvSharp.Point A
        {
            get => a; set
            {
                //parent.Invalidate(new Region(Util.RectAround(a, b).Sys()));
                a = value;
                parent.Invalidate();
            }
        }
        public OpenCvSharp.Point B
        {
            get => b; set
            {
               // parent.Invalidate(new Region(Util.RectAround(a, b).Sys()));
                b = value;
                parent.Invalidate();
            }
        }

        public void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawLine(Pens.Red, a.Sys(), b.Sys());
        }
    }
}

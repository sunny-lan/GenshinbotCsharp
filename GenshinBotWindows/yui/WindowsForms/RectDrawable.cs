using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.WindowsForms
{
    class RectDrawable : Drawable, yui.Rect
    {
        private OpenCvSharp.Rect r;
        private Viewport parent;

        public RectDrawable(Viewport parent)
        {
            this.parent = parent;
        }
        public OpenCvSharp.Rect R
        {
            get => r; set
            {
                //parent.Invalidate(new Region(r.Sys()));
                r = value;
                parent.Invalidate();
            }
        }

        public void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Red, r.Sys());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.WindowsForms
{
    class RectDrawable : Drawable,yui.Rect
    {
        private OpenCvSharp.Rect r;
        private Viewport parent;

        public RectDrawable(Viewport parent)
        {
            this.parent = parent;
        }

        public bool Editable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public OpenCvSharp.Rect R { get =>r; set
            {
                r = value;
                //todo improve efficiency
                parent.Invalidate();
            } }
        

        public void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Red, r.Sys());
        }
    }
}

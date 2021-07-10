using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class RectDrawable : Drawable, yui.Rect
    {
        private OpenCvSharp.Rect r;
        private Viewport parent;

        public RectDrawable(Viewport parent)
        {
            this.parent = parent;
            parent.MouseEvent += Parent_MouseEvent;
        }

        private void Parent_MouseEvent(MouseEvent obj)
        {
            if (r.Contains(obj.Location)) {
                MouseEvent?.Invoke(obj);
            }
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

        public event Action<MouseEvent> ?MouseEvent;

        public void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Red, r.Sys());
        }
    }
}

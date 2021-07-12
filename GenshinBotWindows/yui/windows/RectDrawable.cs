using OpenCvSharp;
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
        }

        public bool Parent_MouseEvent(MouseEvent obj)
        {
            if (r.Contains(obj.Location))
            {
                var tmp = obj;
                tmp.Location -= r.TopLeft;
                MouseEvent?.Invoke(tmp);
                return true;
            }
            return false;
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

        private Pen p=Pens.Red;
        private Scalar color=Scalar.Red;
        public Scalar Color
        {
            get => color; set
            {
                p = new Pen(value.SysBgr255(), 1);
                color = value;
                parent.Invalidate();
            }
        }

        public event Action<MouseEvent>? MouseEvent;

        public void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(p, r.Sys());
        }
    }
}

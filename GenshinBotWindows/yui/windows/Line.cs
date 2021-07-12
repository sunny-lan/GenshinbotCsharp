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

        private Pen p = Pens.Red;
        private Scalar color = Scalar.Red;
        public Scalar Color
        {
            get => color; set
            {
                p = new Pen(value.SysBgr255(), 1);
                color = value;
                parent.Invalidate();
            }
        }
        public void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawLine(p, a.Sys(), b.Sys());
        }

        int perpborder = 3, endBorder=3;

        public event Action<MouseEvent>? MouseEvent;

        public bool Parent_MouseEvent(MouseEvent e)
        {
            //optimize
            //if (!Util.RectAround(a, b).Pad(perpborder).Contains(e.Location))
              //  return false;

            if (a.DistanceTo(b)>endBorder*2)
            {
                var v = e.Location - a;
                Point2d lnV = (b - a);
                var para = v.ProjectOnto(lnV);
                if(para.Length() >=endBorder &&  para.Length()<= lnV.Length())
                {
                    var perp = v - para;
                    if (perp.Length() <= perpborder)
                    {
                        MouseEvent?.Invoke(e);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

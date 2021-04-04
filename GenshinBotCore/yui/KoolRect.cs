using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot.yui
{
    public class KoolRect:yui.Rect,yui.Component
    {
        private Viewport vp;
        private XYLine top, bottom, left, right;
        public KoolRect(Viewport vp)
        {
            this.vp = vp;
            top = XYLine.Create(vp, Orientation.Horizontal);
            bottom = XYLine.Create(vp, Orientation.Horizontal);
            left = XYLine.Create(vp, Orientation.Vertical);
            right = XYLine.Create(vp, Orientation.Vertical);
        }

        public OpenCvSharp.Rect R { get; set; }

        public void Delete()
        {
            top.Delete();
            bottom.Delete();
            left.Delete();
            right.Delete();
        }
    }
}

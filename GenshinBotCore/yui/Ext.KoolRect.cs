using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot.yui
{
    public class KoolRect:yui.Rect,yui.Deletable
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
        public Scalar Color { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event Action<MouseEvent>? MouseEvent
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public void Delete()
        {
            top.Delete();
            bottom.Delete();
            left.Delete();
            right.Delete();
        }
    }
}

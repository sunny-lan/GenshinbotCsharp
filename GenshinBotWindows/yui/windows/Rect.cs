using genshinbot.data;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class Rect : Control, yui.Rect, ViewportComponent
    {
        private OpenCvSharp.Rect _r;
        private Transformation _t;
        public event Action<MouseEvent> MouseEvent
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }
        public OpenCvSharp.Rect R
        {
            get => _r;// => Bounds.Cv();
            set
            {
                _r = value;
                Recalc();
            }// => Bounds = value.Sys();
        }

        public Rect() : base()
        {
            //BackColor = Color.Red;
            //BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var b = Bounds;
            e.Graphics.DrawRectangle(Pens.Red, new Rectangle(0, 0, b.Width - 1, b.Height - 1));
        }

        public void SetTransform(Transformation t)
        {
            _t = t;
            Recalc();
        }

        private void Recalc()
        {
            Bounds = _t.Transform(_r.cvt()).round().Sys();
        }
    }
}

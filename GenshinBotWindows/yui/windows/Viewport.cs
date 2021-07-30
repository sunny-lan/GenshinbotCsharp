using genshinbot.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{

    interface ViewportComponent
    {
        void SetTransform(Transformation t);
    }
    interface Drawable
    {
        bool Parent_MouseEvent(MouseEvent e);
        void OnPaint(PaintEventArgs e);
    }
    class Viewport : Control, yui.Viewport
    {
        private Transformation _transform = Transformation.Unit();

        public event Action<MouseEvent>? MouseEvent;
        OpenCvSharp.Size yui.Viewport.Size
        {
            get => Size.cv();
            set => Invoke((MethodInvoker)delegate
            {
                Size = value.Sys();
            });
        }

        public Transformation T
        {
            get => _transform;
            set
            {

                _transform = value;
                //  Recalc();
                Invalidate();
            }
        }

        public Func<MouseEvent, bool>? OnMouseEvent { get; set; }
        public Action<Transformation>? OnTChange { get; set; }

        public Point2d? MousePos { get; private set; }

        private List<Drawable> drawable = new List<Drawable>();

        internal void Invalidate(OpenCvSharp.Rect r)
        {
            var transformed = T.Transform(r.cvt());
            //TODO not sure if this is less performant
            Invalidate(transformed.Round().Sys());
        }

        protected override bool DoubleBuffered => true;


        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch_bet(Viewport_MouseDown, mouse_wrp(yui.MouseEvent.Kind.Down),e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            switch_bet(Viewport_MouseUp, mouse_wrp(yui.MouseEvent.Kind.Up),e);
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            switch_bet(Viewport_MouseMove, mouse_wrp(yui.MouseEvent.Kind.Click),e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            MousePos = T.Inverse(e.Location.Cv());
            switch_bet(Viewport_MouseMove, mouse_wrp(yui.MouseEvent.Kind.Move),e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Viewport_MouseWheel(this, e);
        }

        Action<MouseEventArgs> mouse_wrp(MouseEvent.Kind kind)
        {
            return (e) =>
            {
                var evt = new MouseEvent
                {
                    Location = T.Inverse(e.Location.Cv()),
                    Type = kind,
                };
                this.MouseEvent?.Invoke(evt);

                for (int i = drawable.Count - 1; i >= 0; i--)
                {
                    if (drawable[i].Parent_MouseEvent(evt)) break;
                }
            };
        }


        void switch_bet<T>(Action<object, T> own, Action<T> other, T e)
        {
            
            if (ModifierKeys.HasFlag(Keys.Alt))
            {
                own(this, e);
            }
            else
            {
                other(e);
            }
        }


        #region dragging

        bool dragging = false;
        private Point2d dragViewPoint;

        private void Viewport_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta == 0) return;
            var mouse = e.Location.Cv();
            OnTChange?.Invoke(T.ScaleAround(mouse, Math.Max(0.1, e.Delta / Math.Abs(e.Delta) * 0.05 + _transform.Scale)));
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {

                OnTChange?.Invoke(T.MatchPoints(dragViewPoint, e.Location.Cv()));
            }
        }

        private void Viewport_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            Console.WriteLine("drag stop");
        }

        private void Viewport_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            Console.WriteLine("drag");
            var mouse = e.Location.Cv();
            dragViewPoint = T.Inverse(mouse);
        }
        #endregion
        private void add(Drawable i)
        {
            lock (drawable) drawable.Add(i);
            Invalidate();
        }


        public yui.Image CreateImage()
        {
            var i = new ImageDrawable(this);
            add(i);
            return i;
        }

        public yui.Rect CreateRect()
        {
            var r = new RectDrawable(this);
            add(r);
            return r;
        }


        public yui.Line CreateLine()
        {
            var l = new Line(this);
            add(l);
            return l;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            // base.OnPaint(e);
            var gfx = e.Graphics;
            gfx.TranslateTransform((float)_transform.Translation.X, (float)_transform.Translation.Y);
            gfx.ScaleTransform((float)_transform.Scale, (float)_transform.Scale);
            lock (drawable)
                foreach (var d in drawable)
                    d.OnPaint(e);
        }

        public void ClearChildren()
        {
            lock (drawable)
                drawable.Clear();
            Invalidate();
        }

        public void Delete(object r)
        {

            lock (drawable)
                if (!drawable.Remove(r as Drawable))
                    throw new Exception();
            Invalidate();

        }
    }
}

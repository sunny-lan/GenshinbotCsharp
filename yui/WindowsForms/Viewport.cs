using GenshinbotCsharp.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
{

    interface ViewportComponent
    {
        void SetTransform(Transformation t);
    }
    interface Drawable
    {
        void OnPaint(PaintEventArgs e);
    }
    class Viewport : Control, yui.Viewport
    {
        private Transformation _transform = Transformation.Unit();


        OpenCvSharp.Size yui.Viewport.Size { get => Size.cv(); set => Size = value.Sys(); }

        private List<ViewportComponent> _controls = new List<ViewportComponent>();
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

        public Func<MouseEvent, bool> OnMouseEvent { get; set; }
        public Action<Transformation> OnTChange { get ; set ; }

        private List<Drawable> drawable = new List<Drawable>();


        void Recalc()
        {
            foreach (var c in _controls)
                c.SetTransform(_transform);
        }

        public Viewport()
        {
            DoubleBuffered = true;
            // BackColor = Color.Black;
            var x = switch_bet(Viewport_MouseUp, mouse_wrp(MouseEvent.Kind.Up));
            MouseUp += (o, e) => x(o, e);
            var y = switch_bet(Viewport_MouseDown, mouse_wrp(MouseEvent.Kind.Down));
            MouseDown += (o, e) => y(o, e);
            var a = switch_bet(Viewport_MouseMove, mouse_wrp(MouseEvent.Kind.Move));
            MouseMove += (o, e) => a(o, e);

            var b = switch_bet(Viewport_MouseWheel, mouse_wrp(MouseEvent.Kind.Up));
            MouseWheel += (o, e) => b(o, e);
        }

        Func<MouseEventArgs, bool> mouse_wrp(MouseEvent.Kind kind)
        {
            return (e) =>
            {
                if (OnMouseEvent == null) return false;
                return OnMouseEvent(new MouseEvent
                {
                    Location = T.Inverse( e.Location.Cv()),
                    Type = kind,
                });
            };
        }

        Action<object, T> switch_bet<T>(Action<object, T> own, Func<T, bool> other)
        {
            return (s, e) =>
            {

                if (ModifierKeys.HasFlag(Keys.Alt))
                {
                    own(s, e);
                }
                else
                {
                    var r = other(e);
                    if (!r) own(s, e);
                }
            };
        }


        #region dragging

        bool dragging = false;
        private Point2d dragViewPoint;

        private void Viewport_MouseWheel(object sender, MouseEventArgs e)
        {
            var mouse = e.Location.Cv();
            OnTChange?.Invoke( T.ScaleAround(mouse, Math.Max(0.1, e.Delta / Math.Abs(e.Delta) * 0.05 + _transform.Scale)));
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
        private void Add<T>(T i) where T : Control, ViewportComponent
        {
            i.SetTransform(this.T);
            _controls.Add(i);
            Controls.Add(i);
        }

        public yui.Image CreateImage()
        {
            var i = new ImageDrawable(this);
            drawable.Add(i);
            return i;
        }

        public yui.Rect CreateRect()
        {
            var r = new RectDrawable(this);
            drawable.Add(r);
            return r;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // base.OnPaint(e);
            var gfx = e.Graphics;
            gfx.TranslateTransform((float)_transform.Translation.X, (float)_transform.Translation.Y);
            gfx.ScaleTransform((float)_transform.Scale, (float)_transform.Scale);
            foreach (var d in drawable)
                d.OnPaint(e);
        }
    }
}

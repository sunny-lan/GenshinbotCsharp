using GenshinbotCsharp.data;
using GenshinbotCsharp;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace genshinbot.windows.badui
{

    class Viewport2 : Control
    {
        private Transformation _transform = Transformation.Unit();


        public Transformation T
        {
            get => _transform;
            set
            {

                _transform = value;
                Invalidate();
            }
        }

        public Action<Transformation> OnTChange { get; set; }

        protected override bool DoubleBuffered => true;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Alt))
                Viewport_MouseWheel(this, e);
            else
                base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Alt))
                Viewport_MouseMove(this, e);
            else
                base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Alt))
                Viewport_MouseDown(this, e);
            else
                base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Alt))
                Viewport_MouseUp(this, e);
            else
                base.OnMouseUp(e);
        }

        #region dragging

        bool dragging = false;
        private Point2d dragViewPoint;

        private void Viewport_MouseWheel(object sender, MouseEventArgs e)
        {
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
        }

        private void Viewport_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            var mouse = e.Location.Cv();
            dragViewPoint = T.Inverse(mouse);
        }
        #endregion
        protected override void OnPaint(PaintEventArgs e)
        {
            // base.OnPaint(e);
            var gfx = e.Graphics;
            gfx.TranslateTransform((float)_transform.Translation.X, (float)_transform.Translation.Y);
            gfx.ScaleTransform((float)_transform.Scale, (float)_transform.Scale);
            
        }
    }
}

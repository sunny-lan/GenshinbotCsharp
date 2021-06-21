using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.Collections.Generic;

namespace genshinbot.yui.windows
{
    class Overlay2:IDisposable
    {
        private GraphicsWindow gw;
        private readonly Dictionary<string, SolidBrush> _brushes;
        private readonly Dictionary<string, Font> _fonts;
        private readonly Dictionary<string, Image> _images;

        public Overlay2()
        {
            _brushes = new Dictionary<string, SolidBrush>();
            _fonts = new Dictionary<string, Font>();
            _images = new Dictionary<string, Image>();

            var gfx = new Graphics()
            {
                MeasureFPS = true,
                PerPrimitiveAntiAliasing = true,
                TextAntiAliasing = true
            };

            gw = new GraphicsWindow(0, 0, 800, 600, gfx)
            {
                FPS = 10,
                IsTopmost = true,
                IsVisible = true,

            };
            gw.SetupGraphics += Gw_SetupGraphics;
            gw.DrawGraphics += Gw_DrawGraphics;
            gw.DestroyGraphics += Gw_DestroyGraphics;
        }

        public IDisposable follow(IObservable<OpenCvSharp.Rect> bounds)
        {
            return bounds.Subscribe(onNext: rr =>
            {
                gw.X = rr.X;
                gw.Y = rr.Y;
                gw.Width = rr.Width;
                gw.Height = rr.Height;
            });
        }
        public IDisposable follow(IObservable<bool> focused)
        {
            return focused.Subscribe(onNext: rr =>
            {
                Console.WriteLine($"visible: {rr}");
                gw.IsVisible = rr;
            });
        }
        public void run()
        {
            gw.Create();
        }

        


        private void Gw_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            foreach (var pair in _brushes) pair.Value.Dispose();
            foreach (var pair in _fonts) pair.Value.Dispose();
            foreach (var pair in _images) pair.Value.Dispose();
        }

        private void Gw_SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            var gfx = e.Graphics;
            if (e.RecreateResources)
            {
                foreach (var pair in _brushes) pair.Value.Dispose();
                foreach (var pair in _images) pair.Value.Dispose();
            }

            _brushes["black"] = gfx.CreateSolidBrush(0, 0, 0);
            _brushes["white"] = gfx.CreateSolidBrush(255, 255, 255);
            _brushes["red"] = gfx.CreateSolidBrush(255, 0, 0);
            _brushes["green"] = gfx.CreateSolidBrush(0, 255, 0);
            _brushes["blue"] = gfx.CreateSolidBrush(0, 0, 255);

            if (e.RecreateResources) return;

            _fonts["arial"] = gfx.CreateFont("Arial", 12);
            _fonts["consolas"] = gfx.CreateFont("Consolas", 30);


        }

        public  OpenCvSharp.Rect? Rect;
        public  OpenCvSharp.Point? Point;
        public Stack<string> Text=new Stack<string>();
        private void Gw_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            var gfx = e.Graphics;
            gfx.ClearScene();
            int idx = Text.Count;
            lock(Text)
            foreach(var entry in Text)
            {
                gfx.DrawText(_fonts["consolas"], _brushes["red"], new Point(0, 50*idx), entry);
                    idx--;
            }
            if (Rect is OpenCvSharp.Rect rr)
                gfx.DrawRectangle(_brushes["red"], new Rectangle(rr.Left, rr.Top, rr.Right, rr.Bottom), 1);
            if(Point is OpenCvSharp.Point p)
            {
                gfx.DrawCircle(_brushes["red"], new Circle(p.X, p.Y, 4), 1);
                gfx.DrawLine(_brushes["red"], p.X, 0, p.X, gw.Height, 1);
                gfx.DrawLine(_brushes["red"], 0, p.Y, gw.Width, p.Y, 1);
            }
        }

        public void Dispose()
        {
            gw.Dispose();
        }
    }
}

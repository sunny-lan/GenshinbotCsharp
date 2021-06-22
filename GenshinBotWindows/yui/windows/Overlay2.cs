using GameOverlay.Drawing;
using GameOverlay.Windows;
using genshinbot.reactive.wire;
using System;
using System.Collections.Generic;

namespace genshinbot.yui.windows
{
    class Overlay2 : IDisposable
    {
        private GraphicsWindow gw;
        private readonly Dictionary<string, SolidBrush> _brushes;
        private readonly Dictionary<string, Font> _fonts;
        private readonly Dictionary<string, Image> _images;

        private bool internalVisible
        {
            get => internalVisible1; set
            {
                internalVisible1 = value;
                gw.IsVisible = internalVisible1 && visible;
            }
        }
        public bool Visible
        {
            get => visible; set
            {
                visible = value;
                gw.IsVisible = internalVisible1 && visible;
            }
        }

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
            };
            gw.SetupGraphics += Gw_SetupGraphics;
            gw.DrawGraphics += Gw_DrawGraphics;
            gw.DestroyGraphics += Gw_DestroyGraphics;

            internalVisible = false;
            Visible = true;
        }

        public IDisposable follow(ILiveWire<OpenCvSharp.Rect?> bounds)
        {
            return bounds.Connect( rrr =>
            {
                if (rrr is OpenCvSharp.Rect rr)
                {
                    Console.WriteLine($"size: {rr}");
                    gw.X = rr.X;
                    gw.Y = rr.Y;
                    gw.Width = rr.Width;
                    gw.Height = rr.Height;
                }
            });
        }
        public IDisposable follow(IWire<bool> focused)
        {
            return focused.Subscribe(rr =>
            {
                Console.WriteLine($"visible: {rr}");
                internalVisible = rr;
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

        public OpenCvSharp.Rect? Rect;
        public OpenCvSharp.Point? Point;
        public Stack<string> Text = new Stack<string>();
        private bool visible;
        private bool internalVisible1;
        public OpenCvSharp.Mat Image
        {
            get => image; set
            {
                if (img!=null) img.Dispose();
                if (value != null)
                    img = gw.Graphics.CreateImage(value.ToBytes());
                else img = null;
                image = value;
            }
        }
        private GameOverlay.Drawing.Image img;
        private OpenCvSharp.Mat image;

        private void Gw_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            var gfx = e.Graphics;
            gfx.ClearScene();
            if (img != null)
                gfx.DrawImage(img, new Point());

            int idx = Text.Count;
            lock (Text)
                foreach (var entry in Text)
                {
                    gfx.DrawText(_fonts["consolas"], _brushes["red"], new Point(0, 50 * idx), entry);
                    idx--;
                }
            if (Rect is OpenCvSharp.Rect rr)
                gfx.DrawRectangle(_brushes["red"], new Rectangle(rr.Left, rr.Top, rr.Right, rr.Bottom), 1);
            if (Point is OpenCvSharp.Point p)
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

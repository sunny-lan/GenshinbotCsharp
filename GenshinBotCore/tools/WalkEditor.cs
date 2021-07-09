using genshinbot.controllers;
using genshinbot.data;
using genshinbot.data.map;
using genshinbot.reactive.wire;
using genshinbot.yui;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class WalkEditor : IDisposable
    {
        YUI ui;
        private readonly LocationManager lm;
        private Tab tab;
        private Viewport vp;
        private Button beginTrack;
        private yui.Rect playerPos;
        private Transformation c2m;

        public WalkEditor(YUI ui, controllers.LocationManager lm)
        {
            this.ui = ui;
            this.lm = lm;
            tab = ui.CreateTab();
            tab.Title = "walk editor";
            tab.Content.SetFlex(new()
            {
                Direction = Orientation.Horizontal,
                Scroll = false,
                Wrap = false
            });

            vp = tab.Content.CreateViewport();
            tab.Content.SetFlex(vp, new()
            {
                Weight = 1
            });
            vp.OnTChange = x => vp.T = x;
            var img = vp.CreateImage();
            img.Mat = Data.MapDb.BigMap.Load();
            img.TopLeft = default;

            var sidebar = tab.Content.CreateSubContainer();
            tab.Content.SetFlex(sidebar, new()
            {
                Weight = 0
            });
            beginTrack = sidebar.CreateButton();
            beginTrack.Text = "track";
            beginTrack.Click += async (s, e) => await BeginTrack_ClickAsync(s, e);


            var features = Data.MapDb.Features;
            c2m = Data.MapDb.Coord2Minimap.Expect();
            foreach (var f in features)
            {
                if (f.Type == FeatureType.Teleporter)
                {
                    var r = vp.CreateRect();
                    r.R = c2m.Transform(f.Coordinates).RectAround(new Size(5, 5));

                }
            }

            playerPos = vp.CreateRect();
        }

        private async Task BeginTrack_ClickAsync(object? sender, EventArgs e)
        {
            beginTrack.Enabled = false;
            IDisposable? d = null;
            var pos = await lm.TrackPos(err =>
              {
                  d?.Dispose();
                  tab.Status = err.ToString();
                  beginTrack.Enabled = true;
              });
            d = pos.Subscribe(coord =>
            {
                playerPos.R = c2m.Transform(coord.Value).RectAround(new Size(3, 3));
            });
            var coord = await pos.Get();
            vp.T = vp.T.MatchPoints(c2m.Transform(coord.Value), vp.Size.Center());
        }

        public void Dispose()
        {
            ui.RemoveTab(tab);
        }
    }
}

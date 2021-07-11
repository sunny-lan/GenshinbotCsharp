using genshinbot.controllers;
using genshinbot.data;
using genshinbot.data.map;
using genshinbot.reactive.wire;
using genshinbot.yui;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class WalkEditor : IDisposable
    {
        public class DB
        {
            public static DbInst<DB> Instance { get; } = new DbInst<DB>("tools/walkeditor.json");


            public class WalkEditPoint
            {
                public Point2d Coord { get; set; }
                public string? Name { get; set; }
                public int ID { get; set; }
            }
            public List<WalkEditPoint> Points { get; set; } = new();

            public int GetNewID()
            {
                return Points.Select(p => p.ID).Max() + 1;
            }
        }
        private DB db = DB.Instance.Value;

        YUI ui;
        private readonly LocationManager lm;
        private Tab tab;
        private Viewport vp;
        private Button beginTrack;
        private yui.Rect? playerInd;
        private Transformation c2m;

        public WalkEditor(YUI ui, controllers.LocationManager lm)
        {
            disable = true;
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

            mapImg = vp.CreateImage();
            mapImg.Mat = Data.MapDb.BigMap.Load();
            mapImg.TopLeft = default;
            mapImg.MouseEvent += e => onMouseEvent(mapImg, e);

            var sidebar = tab.Content.CreateSubContainer();
            tab.Content.SetFlex(sidebar, new()
            {
                Weight = 0
            });

            sidebar.SetFlex(new()
            {
                Direction = Orientation.Vertical,
                Scroll = true,
                Wrap = false,
            });

            beginTrack = sidebar.CreateButton();
            sidebar.SetFlex(beginTrack, new() { Weight = 0 });
            beginTrack.Text = "track";
            beginTrack.Click += BeginTrack_ClickAsync;


            statusLbl = sidebar.CreateLabel();
            sidebar.SetFlex(statusLbl, new() { Weight = 0 });
            statusLbl.Text = "...";

            actionBtn1 = sidebar.CreateButton();
            sidebar.SetFlex(actionBtn1, new() { Weight = 0 });
            actionBtn1.Enabled = false;
            actionBtn1.Text = "";
            actionBtn1.Click += (_, _) => onActionBtn1?.Invoke();

            

            addBtn = sidebar.CreateButton();
            sidebar.SetFlex(actionBtn1, new() { Weight = 0 });
            addBtn.Enabled = false;
            addBtn.Click += AddBtn_Click;

            testBtn = sidebar.CreateButton();
            sidebar.SetFlex(testBtn, new() { Weight = 0 });
            testBtn.Enabled = false;
            testBtn.Text = "test";
            testBtn.Click += TestBtn_Click;



            clearBtn = sidebar.CreateButton();
            sidebar.SetFlex(clearBtn, new() { Weight = 0 });
            clearBtn.Enabled = true;
            clearBtn.Text = "Clear";
            clearBtn.Click += (_,_)=> {
                WholeWalk.Points.Clear();
                updateWalk();
            };


            saveBtn = sidebar.CreateButton();
            sidebar.SetFlex(saveBtn, new() { Weight = 0 });
            saveBtn.Enabled = false;
            saveBtn.Text = "Save";
            saveBtn.Click += SaveBtn_Click;

            loadBtn = sidebar.CreateButton();
            sidebar.SetFlex(loadBtn, new() { Weight = 0 });
            loadBtn.Enabled = true;
            loadBtn.Text = "Load";
            loadBtn.Click += LoadBtn_Click ;


            var features = Data.MapDb.Features;
            c2m = Data.MapDb.Coord2Minimap.Expect();
            foreach (var f in features)
            {
                if (f.Type == FeatureType.Teleporter)
                {
                    var r = vp.CreateRect();
                    r.R = c2m.Transform(f.Coordinates).RectAround(new Size(5, 5));
                    r.MouseEvent += e =>
                    {
                        onMouseEvent(f, e);
                    };

                }
            }
            disable = false;
        }

        private void LoadBtn_Click(object? sender, EventArgs e)
        {
            var res = ui.Popup("load from walk/tmp.json?", "confirm", PopupType.Confirm);
            if (res == PopupResult.Ok)
            {
                var d= Data.ReadJson<LocationManager.WholeWalk>("walk/tmp.json",null);
                if (d == null)
                {
                    tab.Status = "Load failed!";
                    ui.GiveFocus(tab);
                }
                else
                {
                    WholeWalk = d;
                    tab.Status = "Loaded from walk/tmp.json";
                    ui.GiveFocus(tab);
                    updateWalk();
                }
            }
        }

        private async void SaveBtn_Click(object? sender, EventArgs e)
        {
            var res=ui.Popup("save as walk/tmp.json?", "confirm", PopupType.Confirm);
            if (res == PopupResult.Ok)
            {
                await Data.WriteJsonAsync("walk/tmp.json", WholeWalk);
                tab.Status = "Saved into walk/tmp.json";
                ui.GiveFocus(tab);
            }
        }

        private async void TestBtn_Click(object? sender, EventArgs e)
        {
            await lm.WholeWalkTo(WholeWalk);
        }

        LocationManager.WholeWalk WholeWalk = new(null, new());


        private void updateWalk()
        {
            testBtn.Enabled = false; saveBtn.Enabled = false;
            foreach (var item in oldWalkUI)
                vp.Delete(item);
            oldWalkUI = new();
            if (WholeWalk.Points.Count == 0) return;

            if (WholeWalk.Teleporter is not null)
            {
                var ln = vp.CreateLine();
                ln.A = c2m.Transform(WholeWalk.Teleporter.Coordinates).Round();
                ln.B = c2m.Transform(WholeWalk.Points[0].Value).Round();
                oldWalkUI.Add(ln);
                testBtn.Enabled = true; saveBtn.Enabled = true;
            }

            for (int i = 0; i + 1 < WholeWalk.Points.Count; i++)
            {
                var ln = vp.CreateLine();
                ln.A = c2m.Transform(WholeWalk.Points[i].Value).Round();
                ln.B = c2m.Transform(WholeWalk.Points[i + 1].Value).Round();
                oldWalkUI.Add(ln);
            }

        }

        private void AddBtn_Click(object? sender, EventArgs e)
        {
            if (playerInd is not null && selection == playerInd)
            {
                WholeWalk.Points.Add(new LocationManager.WalkPoint(curPPos));

            }
            else if (selection is Feature f)
            {
                if (f.Type == FeatureType.Teleporter)
                {
                    WholeWalk = WholeWalk with { Teleporter = f };
                }
            }
            else if (selection is Point2d p)
            {
                WholeWalk.Points.Add(new LocationManager.WalkPoint(p));
            }
            updateWalk();
        }

        private Point2d curPPos;
        object? selection, lastSel;
        private Label statusLbl;
        private Button actionBtn1;
        private yui.Image mapImg;

        private Action onActionBtn1;
        bool disable = false;
        private Button addBtn;
        private List<object> oldWalkUI=new();
        private Button testBtn;
        private Button clearBtn;
        private Button saveBtn;
        private Button loadBtn;

        void onMouseEvent(object o, MouseEvent e)
        {
            if (disable) return;
            if (e.Type == MouseEvent.Kind.Click)
            {
                if (o is Feature f1)
                {
                    selection = f1;
                }
                if (o == mapImg)
                {
                    selection = c2m.Inverse( e.Location);
                }
                if (playerInd is not null && o == playerInd)
                {
                    selection = playerInd;
                }
            }


            actionBtn1.Enabled = false;
            addBtn.Enabled = false;
            addBtn.Text = "";
            statusLbl.Text = "...";

            if (playerInd is not null && selection == playerInd)
            {
                statusLbl.Text = $"player";
                addBtn.Enabled = true;
                addBtn.Text = "add";

            }
            else if (selection is Feature f)
            {
                statusLbl.Text = $"{f.ID}:{f.Type}";
                if (f.Type == FeatureType.Teleporter)
                {
                    actionBtn1.Text = "Teleport";
                    actionBtn1.Enabled = true;
                    addBtn.Enabled = true;
                    addBtn.Text = "Set teleporter";

                    onActionBtn1 = async () =>
                     {
                         disable = true;
                         actionBtn1.Enabled = false;
                         try
                         {
                             lm.screens.io.W.TryFocus();
                             if (lm.screens.ActiveScreen.Value == lm.screens.PlayingScreen)
                                 await lm.screens.PlayingScreen.OpenMap();
                             await lm.screens.MapScreen.TeleportTo(f);
                         }
                         finally
                         {
                             actionBtn1.Enabled = true; disable = false;
                         }
                     };
                }
            }
            else if (selection is Point2d p)
            {

                statusLbl.Text = $"MapPoint";
                addBtn.Text = "add";
                addBtn.Enabled = true;
            }
            
        }

        private async void BeginTrack_ClickAsync(object? sender, EventArgs e)
        {
            beginTrack.Enabled = false;
            IDisposable? d = null;
            var pos = await lm.TrackPos(err =>
            {
                vp.Delete(playerInd!);
                playerInd = null;
                d?.Dispose();
                tab.Status = err.ToString();
                ui.GiveFocus(tab);
                beginTrack.Enabled = true;
            });

            playerInd = vp.CreateRect();
            playerInd.MouseEvent += e => onMouseEvent(playerInd, e);

            d = pos.Subscribe(coord =>
            {
                curPPos =coord.Value;
                playerInd.R = c2m.Transform(curPPos).RectAround(new Size(3, 3));
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

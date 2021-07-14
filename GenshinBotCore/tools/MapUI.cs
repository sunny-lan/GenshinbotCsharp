using genshinbot.controllers;
using genshinbot.data;
using genshinbot.data.map;
using genshinbot.reactive.wire;
using genshinbot.util;
using genshinbot.yui;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class MapUI : IDisposable
    {



        readonly YUI ui;
        private readonly yui.Tab tab;
        private readonly Viewport vp;
        private readonly Transformation c2m;
        private readonly LiveWireSource<object?> selected = new(null);
        private readonly LiveWireSource<bool> editing = new(false);
        private readonly LiveWireSource<Point2d?> cursorPos = new(null);
        private readonly Container sidebar;
        private readonly Button actionBtn1;
        private readonly Action? onActionBtn1;

        public MapUI(YUI ui, LocationManager? lm)
        {
            Data.MapDb.CalculateCoord2Minimap();//todo
            c2m = Data.MapDb.Coord2Minimap.Expect();

            this.ui = ui;
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
            vp.T = vp.T.MatchPoints(c2m.Transform(Data.MapDb.Features[0].Coordinates), vp.Size.Center());

            //  mapImg.MouseEvent += e => onMouseEvent(mapImg, e);

            sidebar = tab.Content.CreateSubContainer();
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

            /*      beginTrack = sidebar.CreateButton();
                  sidebar.SetFlex(beginTrack, new() { Weight = 0 });
                  beginTrack.Text = "track";
                  beginTrack.Click += BeginTrack_ClickAsync;*/



            /* ;



             testBtn = sidebar.CreateButton();
             sidebar.SetFlex(testBtn, new() { Weight = 0 });
             testBtn.Enabled = false;
             testBtn.Text = "test";
             testBtn.Click += TestBtn_Click;
*/


            initEditBtn();
            initAddBtn();
            initDeleteBtn();
            initSaveLoad();
            initStatusLbl();

            /*actionBtn1 = sidebar.CreateButton();
            sidebar.SetFlex(actionBtn1, new() { Weight = 0 });
            actionBtn1.Enabled = false;
            actionBtn1.Text = "";
            actionBtn1.Click += (_, _) => onActionBtn1?.Invoke();*/

            if (lm is LocationManager ll)
            {
                initTesting(ll);
            }

            selected.Subscribe(x =>
            {
                rerenderGraph();
            });

            initMapImg(ui);
            rerenderGraph();
            initCursor();
        }

        void initTesting(LocationManager lm)
        {

            var testBtn = sidebar.CreateButton();
            sidebar.SetFlex(testBtn, new() { Weight = 0 });
            var running = new LiveWireSource<bool>(false);
            Wire.Combine(running, selected, (run, sel) =>
                 !run && sel is Feature).Connect(en => testBtn.Enabled = en);
            testBtn.Text = "test";
            testBtn.Click += async(object? sender, EventArgs e) =>
            {
                try
                {
                    running.SetValue(true);
                    var dst = (Feature)selected.Value!;
                    await lm.Goto(dst);
                }catch(Exception E)
                {
                    tab.Status = E.Message;
                    ui.GiveFocus(tab);
                }
                finally
                {
                    running.SetValue(false);
                }
            };
        }


        private void initSaveLoad()
        {

            var saveBtn = sidebar.CreateButton();
            sidebar.SetFlex(saveBtn, new() { Weight = 0 });
            saveBtn.Enabled = true;
            saveBtn.Text = "Save";
            saveBtn.Click += async (_, _) =>
            {
                var res = ui.Popup($"save into {MapDb.Instance.DbFilePath }? ", "confirm", PopupType.Confirm);
                if (res == PopupResult.Ok)
                {
                    await MapDb.Instance.Save();
                    tab.Status = $"Saved into {MapDb.Instance.DbFilePath}";
                    ui.GiveFocus(tab);
                }
            };

            var loadBtn = sidebar.CreateButton();
            sidebar.SetFlex(loadBtn, new() { Weight = 0 });
            loadBtn.Enabled = true;
            loadBtn.Text = "Load";
            loadBtn.Click += (_, _) =>
            {
                var res = ui.Popup($"reload from {MapDb.Instance.DbFilePath }? ", "confirm", PopupType.Confirm);
                if (res != PopupResult.Ok) return;

                //make sure everything is clear
                selected.SetValue(null);
                editing.SetValue(false);

                MapDb.Instance.ReloadFromDisk();
                rerenderGraph();
            };
        }

        private void initMapImg(YUI ui)
        {
            var mapImg = vp.CreateImage();
            mapImg.Mat = Data.MapDb.BigMap.Load();
            mapImg.TopLeft = default;
            mapImg.MouseEvent += e =>
            {
                var obj = c2m.Inverse(e.Location);
                if (e.Type == MouseEvent.Kind.Click)
                {
                    if (editing.Value)
                    {
                        var editted = (Feature)selected.Value!;
                        if (editted.Type != FeatureType.RandomPoint)
                            if (ui.Popup($"do you really want to move {editted.Type}??",
                                "confirm", PopupType.Confirm) == PopupResult.Cancel)
                                return;
                        editted.Coordinates = obj;
                        rerenderGraph();

                    }
                    else
                    {
                        cursorPos.SetValue(obj);
                    }
                }
            };
        }

        private void initDeleteBtn()
        {
            var deleteBtn = sidebar.CreateButton();
            deleteBtn.Text = "delete";
            selected.Connect(x =>
            {
                deleteBtn.Enabled = x is Feature or (Feature, int);
            });
            deleteBtn.Click += (_, _) =>
            {
                object? newSel = null;
                if (selected.Value is Feature f)
                {
                    if (f.Type != FeatureType.RandomPoint)
                        if (ui.Popup($"do you really want to delete {f.Type}??",
                                   "confirm", PopupType.Confirm) == PopupResult.Cancel)
                            return;
                    newSel = Data.MapDb.DeleteFeature(f.ID);
                }
                else if (selected.Value is (Feature src, int dst))
                {
                    src.Reachable?.Remove(dst);
                    newSel = src;
                }
                else Debug.Fail("");
                selected.SetValue(newSel);
            };
        }

        private void initCursor()
        {
            yui.Rect? cursor = null;
            cursorPos.Connect(pt =>
            {
                if (pt is null)
                {
                    if (cursor is not null)
                        vp.Delete(cursor);
                    cursor = null;
                }
                else
                {
                    if (cursor is null)
                    {
                        cursor = vp.CreateRect();
                        cursor.Color = Scalar.Cyan;
                    }
                    cursor.R = c2m.Transform(pt.Expect()).RectAround(new Size(3, 3));
                }
            });
        }

        private void initAddBtn()
        {
            var addBtn = sidebar.CreateButton();
            addBtn.Text = "Add";
            Wire.Combine(cursorPos, selected, (pos, sel) =>
            {
                return pos is not null;
            }).Connect(en => addBtn.Enabled = en);
            addBtn.Click += (_, _) =>
            {
                Feature newFeat = new Feature
                {
                    Coordinates = cursorPos.Value.Expect(),
                    Type = FeatureType.RandomPoint,
                };
                Data.MapDb.Features.Add(newFeat);

                if (selected.Value is Feature src)
                {
                    src.Reachable ??= new();
                    src.Reachable.Add(newFeat.ID);
                }

                selected.SetValue(newFeat);
                cursorPos.SetValue(null);
            };
        }

        private void initEditBtn()
        {
            var editBtn = sidebar.CreateButton();
            sidebar.SetFlex(editBtn, new() { Weight = 0 });
            editing.Connect(b =>
            {
                editBtn.Text = b ? "Stop Edit" : "edit";
            });//todo dispose
            editBtn.Click += (_, _) => editing.SetValue(!editing.Value);
            selected.Connect(o =>
            {
                //check editable object
                editBtn.Enabled = o is Feature;
            });
        }

        void initStatusLbl()
        {
            var statusLbl = sidebar.CreateLabel();
            sidebar.SetFlex(statusLbl, new() { Weight = 0 });
            selected.Connect(sel =>
            {
                switch (sel)
                {
                    case Feature f:
                        statusLbl.Text = $"{f.Type}\n{f.Coordinates.Round()}\n{f.Name ?? f.ID.ToString()}";
                        break;
                    case (Feature src, int dst):
                        statusLbl.Text = $"{src.Name ?? src.ID.ToString()}->{dst}";
                        break;
                    default:
                        statusLbl.Text = "null";
                        break;
                }
            });
        }

        List<object> oldUi = new();

        void rerenderGraph()
        {
            foreach (var obj in oldUi) vp.Delete(obj);
            oldUi.Clear();

            var features = Data.MapDb.Features;
            var mapping = new Dictionary<int, Feature>();
            foreach (var f in features)
            {
                mapping[f.ID] = f;
            }

            //draw edges
            var sel1 = selected.Value as (Feature src, int dst)?;
            foreach (var f in features)
            {
                if (f.Reachable is null) continue;


                foreach (var id in f.Reachable)
                {
                    var adj = mapping[id];

                    var ln = vp.CreateLine();
                    oldUi.Add(ln);

                    var obj = (f, id);
                    if (sel1 == obj)
                    {
                        ln.Color = Scalar.Blue;
                    }

                    ln.A = c2m.Transform(f.Coordinates).Round();
                    ln.B = c2m.Transform(adj.Coordinates).Round();
                    ln.MouseEvent += e =>
                    {
                        if (e.Type == MouseEvent.Kind.Click)
                        {
                            if (editing.Value)
                            {
                            }
                            else
                            {
                                selected.SetValue(obj);
                            }
                        }
                    };
                }
            }

            //draw nodes
            var sel = selected.Value as Feature;
            foreach (var f in features)
            {

                var r = vp.CreateRect();
                oldUi.Add(r);
                if (sel?.ID == f.ID)
                {
                    r.Color = Scalar.Blue;
                }

                r.R = c2m.Transform(f.Coordinates).RectAround(new Size(5, 5));

                r.MouseEvent += e =>
                {
                    if (e.Type == MouseEvent.Kind.Click)
                    {
                        if (editing.Value)
                        {
                            var src = (Feature)selected.Value!;
                            //disallow loops or even dag
                            if (Data.MapDb.FindPath(f.ID, src.ID) is null &&
                                Data.MapDb.FindPath(src.ID, f.ID) is null
                            )
                            {
                                src.Reachable ??= new();
                                src.Reachable.Add(f.ID);
                                selected.SetValue(f);//for ease of editing
                            }
                            else
                            {
                                selected.SetValue(f);
                            }
                        }
                        else
                        {
                            selected.SetValue(f);
                        }
                    }
                };
            }


        }

        public void Dispose()
        {
            //todo ui.RemoveTab(tab);
        }

    }
}

using genshinbot.automation;
using genshinbot.screens;
using genshinbot;
using genshinbot.yui;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools.config
{
    class PlayingScreenConfig : Script
    {
        Tab tab;
        public void Load(GenshinBot b)
        {
            Debug.Assert(tab == null);
            Debug.Assert(b.Ui != null);
            Debug.Assert(b.Db != null);

            var ui = b.Ui;
            var db = b.Db.PlayingScreenDb;
            tab = ui.CreateTab();
            tab.Title = "Playing screen";
            var content = tab.Content;
            var vp = tab.Content.CreateViewport();
            vp.Size = new Size(500, 500);
            vp.OnTChange = t => vp.T = t;
            var img = vp.CreateImage();
            var screenshotBtn = content.CreateButton();
            screenshotBtn.Text = "Screenshot";

            //only enable button when attached
            screenshotBtn.Enabled = b.W != null;
            b.AttachedWindowChanged += (s, attached) => screenshotBtn.Enabled = attached;

            var repaintList = new List<XYLine>();

            PlayingScreen.Db.RD activeRD;

            void repaint()
            {
                //clear old ui
                foreach (var xyline in repaintList)
                    xyline.Delete();
                repaintList.Clear();

                if (activeRD == null) return;

                void createLine(int? _v, Orientation o, int? min = null, int? max = null)
                {
                    if (_v is int v)
                    {
                        var line = XYLine.Create(vp, o, min, max);
                        line.V = v;
                        repaintList.Add(line);
                    }
                }

                var template = activeRD.CharTemplate;

                createLine(template.HealthXBegin, Orientation.Vertical);
                createLine(template.HealthXEnd, Orientation.Vertical);
                createLine(template.NumberXBegin, Orientation.Vertical);
                createLine(template.NumberXEnd, Orientation.Vertical);

                for (int i = 0; i < 4; i++)
                {
                    int? offset = activeRD.TemplateYOffset[i];
                    createLine(template.HealthY + offset, Orientation.Horizontal, template.HealthXBegin, template.HealthXEnd);
                    createLine(template.NumberYBegin + offset, Orientation.Horizontal, template.NumberXBegin, template.NumberXEnd);
                    createLine(template.NumberYEnd + offset, Orientation.Horizontal, template.NumberXBegin, template.NumberXEnd);
                }
            }

            var prevSize = new Size();
            screenshotBtn.Click += async (s, e) =>
            {
                var rect = await b.W.GetBoundsAsync();
                var screenshot = await b.W.ScreenshotAsync(rect);
                img.Mat = screenshot;

                var size = rect.Size;
                if (size != prevSize)
                {

                    prevSize = size;
                    if (!db.R.ContainsKey(size))
                        db.R[size] = new PlayingScreen.Db.RD();
                    activeRD = db.R[size];


                    async Task askUser(string selectionMessage, int? val, Action<int> set, Orientation o)
                    {
                        if (val is null)
                        {
                            tab.Status = selectionMessage;
                            ui.GiveFocus(tab);
                            set(await vp.SelectXY(o));
                            repaint();
                        }

                    }

                    Debug.Assert(activeRD.TemplateYOffset != null);
                    repaint();

                    var template = activeRD.CharTemplate;
                    await askUser("Health X begin", template.HealthXBegin, v => template.HealthXBegin = v, Orientation.Vertical);
                    await askUser("Health X end", template.HealthXEnd, v => template.HealthXEnd = v, Orientation.Vertical);
                    await askUser("Number X begin", template.NumberXBegin, v => template.NumberXBegin = v, Orientation.Vertical);
                    await askUser("Number X end", template.NumberXEnd, v => template.NumberXEnd = v, Orientation.Vertical);

                    await askUser("Number Y begin", template.NumberYBegin, v => template.NumberYBegin = v, Orientation.Horizontal);
                    await askUser("Number Y end", template.NumberYEnd, v => template.NumberYEnd = v, Orientation.Horizontal);

                    await askUser("C0 Health Y", template.HealthY, v => template.HealthY = v, Orientation.Horizontal);

                    for (int i = 1; i < 4; i++)
                    {
                        await askUser("C" + i + " Health Y", activeRD.TemplateYOffset[i], y =>
                        {
                            activeRD.TemplateYOffset[i] = y - template.HealthY;
                            repaint();
                        }, Orientation.Horizontal);
                    }


                    tab.Status = "Idle";
                }
            };


        }

        public void Unload(GenshinBot b)
        {
            Debug.Assert(tab != null);
            b.Ui.RemoveTab(tab);
            tab = null;
        }
    }
}

using genshinbot.automation;
using genshinbot.screens;
using genshinbot;
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
        yui.Tab tab;
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


            List<yui.Rect> displayCharTemplates(PlayingScreen.Db.RD.CharacterTemplate[] templates)
            {
                Debug.Assert(templates.Length == 4);


                var res = new List<yui.Rect>();
                for (int i = 0; i < 4; i++)
                {
                    var character = templates[i];

                    var health = vp.CreateRect();
                    health.R = character.Health;
                    res.Add(health);

                    var num = vp.CreateRect();
                    num.R = character.Number;
                    res.Add(num);
                }
                return res;
            }

            async Task<PlayingScreen.Db.RD.CharacterTemplate[]> askCharacterTemplates()
            {
                var prevStatus = tab.Status;
                var lines = new List<yui.XYLine>();

                async Task<int> askUser(string selectionMessage, yui.Orientation o, int? min = null, int? max = null)
                {
                    tab.Status = selectionMessage;
                    ui.GiveFocus(tab);
                    var r = await yui.XYLine.Select(vp, o, min, max);
                    lines.Add(r);
                    return r.V;
                }


                int nxb = await askUser("Number X begin", yui.Orientation.Vertical);
                int nxe = await askUser("Number X end", yui.Orientation.Vertical);
                if (nxe < nxb) Util.Swap(ref nxe, ref nxb);

                int hxb = await askUser("Health X begin", yui.Orientation.Vertical);
                int hxe = await askUser("Health X end", yui.Orientation.Vertical);
                if (hxe < hxb) Util.Swap(ref hxb, ref hxe);

                var res = new PlayingScreen.Db.RD.CharacterTemplate[4];
                for (int i = 0; i < 4; i++)
                {
                    int nyb = await askUser(i + " Number Y begin", yui.Orientation.Horizontal, nxb, nxe);
                    int nye = await askUser(i + " Number Y end", yui.Orientation.Horizontal, nxb, nxe);
                    if (nye < nyb) Util.Swap(ref nyb, ref nye);

                    var number = new Rect(nxb, nyb, nxe - nxb, nye - nyb);

                    int hyb = await askUser(i + " Health Y begin", yui.Orientation.Horizontal, hxb, hxe);
                    int hye = await askUser(i + " Health Y end", yui.Orientation.Horizontal, hxb, hxe);
                    if (hye < hyb) Util.Swap(ref hyb, ref hye);

                    var health = new Rect(hxb, hyb, hxe - hxb, hye - hyb);

                    res[i] = new PlayingScreen.Db.RD.CharacterTemplate { Health = health, Number = number };
                }

                foreach (var l in lines) l.Delete();
                tab.Status = prevStatus;

                return res;
            }

            List<yui.Rect> prevRects = null;
            PlayingScreen.Db.RD activeRD = null;

            var clearBtn = content.CreateButton();
            clearBtn.Text = "Clear";
            clearBtn.Click += (s, e) =>
            {
                setCharTemplates(null);
            };

            var saveBtn = content.CreateButton();
            saveBtn.Text = "Save";
            saveBtn.Click += async(s, e) =>
            {
                await Task.Run(()=>b.Db.SavePlayingScreenDb());
                tab.Status = "Saved";
            };


            void setCharTemplates(PlayingScreen.Db.RD.CharacterTemplate[] templates)
            {
                Debug.Assert(activeRD != null);
                activeRD.Characters = templates;
                if (activeRD.Characters == null)
                {
                    clearBtn.Enabled = false;
                    prevRects?.ForEach(r => vp.Delete(r));
                    prevRects?.Clear();

                    askCharacterTemplates().ContinueWith(x => setCharTemplates(x.Result));
                }
                else
                {
                    clearBtn.Enabled = true;
                    prevRects = displayCharTemplates(activeRD.Characters);
                }
            }

            void setActiveRD(PlayingScreen.Db.RD rd)
            {
                activeRD = rd;
                if (activeRD == null)
                {
                    clearBtn.Enabled = false;
                }
                else
                {
                    setCharTemplates(activeRD.Characters);
                }
            }

            var prevSize = new Size();
            screenshotBtn.Click += async (s, e) =>
            {
                screenshotBtn.Enabled = false;
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

                    setActiveRD(activeRD);

                }
                screenshotBtn.Enabled = true;
            };

            setActiveRD(null);

        }

        public void Unload(GenshinBot b)
        {
            Debug.Assert(tab != null);
            b.Ui.RemoveTab(tab);
            tab = null;
        }
    }
}

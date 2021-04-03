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

            var uiRects = new List<genshinbot.yui.Rect>();

            var prevSize = new Size();
            screenshotBtn.Click += async (s, e) =>
            {
                var rect = await b.W.GetBoundsAsync();
                var screenshot = await b.W.ScreenshotAsync(rect);
                img.Mat = screenshot;

                var size = rect.Size;
                if (size != prevSize)
                {
                    //clear old ui
                    foreach (var uiRect in uiRects)
                        vp.Delete(uiRect);
                    uiRects.Clear();

                    prevSize = size;
                    if (!db.R.ContainsKey(size))
                        db.R[size] = new PlayingScreen.Db.RD();
                    var r = db.R[size];

                    var characters=new PlayingScreen.Db.RD.CharacterConfig[4];

                    for (int i = 0; i < 4; i++)
                    {
                        genshinbot.yui.Rect name, number;
                        var character = r.Characters[i];
                       // if (character == null)
                        {
                            //TODO
                            character = r.Characters[i] = new PlayingScreen.Db.RD.CharacterConfig();
                        }
                        tab.Status = "Select character " + i + " name";
                        ui.GiveFocus(tab);
                        uiRects.Add(name = await vp.SelectAndCreate());
                        character.Name = name.R;

                        tab.Status = "Select character " + i + " number";
                        ui.GiveFocus(tab);
                        uiRects.Add(number = await vp.SelectAndCreate());
                        character.Number = number.R;


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

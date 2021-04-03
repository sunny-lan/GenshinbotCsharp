using GenshinbotCsharp;
using GenshinbotCsharp.yui;
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
            var img = vp.CreateImage();
            var screenshotBtn = content.CreateButton();
            screenshotBtn.Text = "Screenshot";

            var uiRects = new List<GenshinbotCsharp.yui.Rect>();

            var prevSize = new Size();
            screenshotBtn.Click += async (s, e) =>
            {
                var rect = b.W.GetBounds();
                var screenshot = await (b.W as IWindowAutomator).ScreenshotAsync(rect);
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
                        db.R[size] = new GenshinbotCsharp.screens.PlayingScreen.Db.RD();
                    var r = db.R[size];
                    for (int i = 0; i < 4; i++)
                    {
                        GenshinbotCsharp.yui.Rect name, number;
                        var character = r.Characters[i];
                        if (character == null)
                        {
                            tab.Status = "Select character " + i + " name";
                            uiRects.Add(name = await vp.SelectAndCreate());
                            character.Name = name.R;

                            tab.Status = "Select character " + i + " number";
                            uiRects.Add(number = await vp.SelectAndCreate());
                            character.Number = number.R;
                        }
                        else
                        {
                            name = vp.CreateRect();
                            name.R = character.Name;

                            number = vp.CreateRect();
                            number.R = character.Name;
                        }


                    }
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

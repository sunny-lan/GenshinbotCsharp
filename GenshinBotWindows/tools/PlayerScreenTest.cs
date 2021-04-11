using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    class PlayerScreenTest : Script
    {
        yui.Tab tab;

        public string DisplayName => "Playing screen test";

        public void Load(GenshinBot b)
        {
            Debug.Assert(tab == null);
            Debug.Assert(b.Ui != null);
            Debug.Assert(b.Db != null);
            Debug.Assert(b.W != null);
            Debug.Assert(b.PlayingScreen != null);

            tab = b.Ui.CreateTab();
            tab.Title = "Player screen test";
            var content = tab.Content;
            content.SetFlex(new yui.Flexbox
            {
                Direction = yui.Orientation.Vertical,
                Scroll = false,
                Wrap = false,
            });
            var btnContainer = content.CreateSubContainer(); 
            btnContainer.SetFlex(new yui.Flexbox
            {
                Direction = yui.Orientation.Horizontal,
                Scroll = false,
                Wrap = false,
            });

            var health = new yui.Label[4];
            var num = new yui.Label[4];
            var alive = new yui.Label[4];
            for (int i = 0; i < 4; i++)
            {
                var row = content.CreateSubContainer();
                row.SetFlex(new yui.Flexbox
                {
                    Direction = yui.Orientation.Horizontal,
                    Scroll = false,
                    Wrap = false,
                });

                var idxLbl = row.CreateLabel();
                row.SetFlex(idxLbl, new yui.Flexbox.Item { Weight = 0 });
                idxLbl.Text = i.ToString();

                health[i] = row.CreateLabel();
                row.SetFlex(health[i], new yui.Flexbox.Item { Weight = 1 });
                num[i] = row.CreateLabel();
                row.SetFlex(health[i], new yui.Flexbox.Item { Weight = 1 });
                alive[i] = row.CreateLabel();
                row.SetFlex(health[i], new yui.Flexbox.Item { Weight = 1 });
            }

            var playingScreen = b.PlayingScreen;

          
            void makePoller(string text, Action<int> fn)
            {
                bool running = false;

                void pollStatus()
                {
                    while (running)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            fn(i);
                        }
                    }
                }

                var btn = btnContainer.CreateButton();
                btn.Text =$"start {text}";
                btnContainer.SetFlex(btn, new yui.Flexbox.Item { Weight = 1 });

                Task poller = null;
                btn.Click += async (s, e) =>
               {
                   btn.Enabled = false;
                   if (running)
                   {
                       running = false;
                       await poller;
                       btn.Text = $"start {text}";
                   }
                   else
                   {
                       btn.Text = $"stop {text}";
                       running = true;
                       poller = Task.Run(pollStatus);
                   }
                   btn.Enabled = true;
               };
            }

            makePoller("health", i=> health[i].Text = playingScreen.ReadSideHealth1(i).ToString());
            makePoller("num", i=> num[i].Text = playingScreen.ReadCharSelected(i).ToString());
            makePoller("alive", i=> alive[i].Text = playingScreen.ReadSideAlive(i).ToString());
        }

        public void Unload(GenshinBot b)
        {
            Debug.Assert(tab != null);
            b.Ui.RemoveTab(tab);
            tab = null;
        }
    }
}

using genshinbot.automation;
using genshinbot;
using genshinbot.yui;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    class BotControlPanel : Script
    {
        HashSet<Script> scripts = new HashSet<Script> {
            new genshinbot.tools.config.PlayingScreenConfig(),
            new genshinbot.tools.PlayerScreenTest(),
        };

        Tab tab;

        public string DisplayName => "Script list";

        public void Load(GenshinBot b)
        {
            Debug.Assert(tab == null);
            Debug.Assert(b.Ui != null);
            Debug.Assert(b.Db != null);

            var ui = b.Ui;
            tab = ui.CreateTab();
            tab.Title = "Control panel";
            var content = tab.Content;
            content.SuspendLayout();
            content.SetFlex(new yui.Flexbox { Direction = Orientation.Horizontal });
            var scriptList = content.CreateTreeview();
            content.SetFlex(scriptList, new Flexbox.Item { Weight = 1 });

            var sidebar = content.CreateSubContainer();
            content.SetFlex(sidebar, new Flexbox.Item { Weight = 0 });
            sidebar.SetFlex(new yui.Flexbox { Direction = Orientation.Vertical });

            var loadUnloadBtn = sidebar.CreateButton();
            Action onClick = null;
            loadUnloadBtn.Click += (s, e) => onClick?.Invoke();
            loadUnloadBtn.Enabled = false;
            var attach = sidebar.CreateButton();
            attach.Text = "Attach to window";

            scriptList.BeginUpdate();
            foreach (var script in scripts)
            {
                var node = scriptList.CreateNode();
                node.Text = script.DisplayName;

                Action<bool> update = loaded =>
                {
                    loadUnloadBtn.Text = loaded ? "Unload" : "Load";
                    node.Color = loaded?Scalar.Green:Scalar.Gray;
                };


                node.Selected += (s, e) =>
                {
                    bool loaded = b.IsLoaded(script);
                    loadUnloadBtn.Enabled = true;
                    update(loaded);
                    onClick = () =>
                    {
                        loadUnloadBtn.Enabled = false;
                        if (loaded)
                        {
                            b.Unload(script);
                            loaded = false;
                            update(loaded);
                        }
                        else
                        {
                            try
                            {
                                b.Load(script);
                                loaded = true;
                                update(loaded);
                            }
                            catch(Exception e)
                            {
                                ui.Popup(e.ToString(),"Failed to load script");
                            }
                        }
                        loadUnloadBtn.Enabled = true;
                    };
                };
            }
            scriptList.EndUpdate();

           
            attach.Click += (s, e) =>
            {
                try
                {
                    b.AttachWindow();
                }
                catch (AttachWindowFailedException ex)
                {
                    ui.Popup(ex.ToString(), "Error!");
                }
            };
            attach.Enabled = b.W == null;
            b.AttachedWindowChanged += (s, attached) => attach.Enabled = !attached;
            content.ResumeLayout();
        }

        public void Unload(GenshinBot b)
        {
            Debug.Assert(tab != null);
            b.Ui.RemoveTab(tab);
            tab = null;
        }
    }
}

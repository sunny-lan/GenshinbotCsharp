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
            var scriptList = content.CreateTreeview();
            var loadUnloadBtn = content.CreateButton();
            Action onClick = null;
            loadUnloadBtn.Click += (s, e) => onClick?.Invoke();
            loadUnloadBtn.Enabled = false;

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
                        }
                        else
                        {
                            b.Load(script);
                            loaded = true;
                        }
                        update(loaded);
                        loadUnloadBtn.Enabled = true;
                    };
                };
            }
            scriptList.EndUpdate();

            var attach = content.CreateButton();
            attach.Text = "Attach to window";
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
        }

        public void Unload(GenshinBot b)
        {
            Debug.Assert(tab != null);
            b.Ui.RemoveTab(tab);
            tab = null;
        }
    }
}

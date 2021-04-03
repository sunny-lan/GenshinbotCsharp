using genshinbot.core.automation;
using GenshinbotCsharp;
using GenshinbotCsharp.yui;
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
            var unloaded = content.CreateTreeview();
            var loaded = content.CreateTreeview();
            var loadUnloadBtn = content.CreateButton();
            Action onClick = null;
            loadUnloadBtn.Click += (s, e) => onClick?.Invoke();
            loadUnloadBtn.Enabled = false;

            var attach = content.CreateButton();
            attach.Text = "Attach to window";
            attach.Click += (s, e) =>
            {
                try
                {
                    b.AttachWindow();
                }
                catch (AttachWindowFailedException ex) {
                    ui.Popup(ex.ToString(), "Error!");
                }
            };
            attach.Enabled = b.W == null;
            b.AttachedWindowChanged += (s, attached) => attach.Enabled = !attached;

            Action<Script> addToLoaded = null;
            Action<Script> addToUnloaded = script =>
            {
                var node = unloaded.CreateNode();
                node.Text = script.DisplayName;
                node.Selected += (s, e) =>
                {
                    loadUnloadBtn.Enabled = true;
                    loadUnloadBtn.Text = "Load";
                    onClick = () =>
                    {
                        loadUnloadBtn.Text = "";
                        onClick = null;
                        loadUnloadBtn.Enabled = false;
                        unloaded.Delete(node);
                        b.Load(script);
                        addToLoaded(script);
                    };
                };
            };

            addToLoaded = script =>
            {
                var node = loaded.CreateNode();
                node.Text = script.DisplayName;
                node.Selected += (s, e) =>
                {
                    loadUnloadBtn.Enabled = true;
                    loadUnloadBtn.Text = "Unload";
                    onClick = () =>
                    {
                        loadUnloadBtn.Text = "";
                        onClick = null;
                        loadUnloadBtn.Enabled = false;
                        loaded.Delete(node);
                        b.Unload(script);
                        addToUnloaded(script);
                    };
                };
            };

            unloaded.BeginUpdate();
            foreach (var script in scripts)
            {
                addToUnloaded(script);
            }
            unloaded.EndUpdate();
        }

        public void Unload(GenshinBot b)
        {
            Debug.Assert(tab != null);
            b.Ui.RemoveTab(tab);
            tab = null;
        }
    }
}

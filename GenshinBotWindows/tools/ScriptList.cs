using GenshinbotCsharp;
using GenshinbotCsharp.yui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    class ScriptList : Script
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
            tab.Title = "Scripts";
            var content = tab.Content;
            var unloaded = content.CreateTreeview();
            var loaded = content.CreateTreeview();
            var btn = content.CreateButton();
            Action onClick = null;
            btn.Click += (s, e) => onClick?.Invoke();

            Action<Script> addToLoaded = null;
            Action<Script> addToUnloaded = script =>
            {
                var node = unloaded.CreateNode();
                node.Text = script.DisplayName;
                node.Selected += (s, e) =>
                {
                    btn.Text = "Load";
                    onClick = () =>
                    {
                        btn.Text = "";
                        onClick = null;
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
                    btn.Text = "Unload";
                    onClick = () =>
                    {
                        btn.Text = "";
                        onClick = null;
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

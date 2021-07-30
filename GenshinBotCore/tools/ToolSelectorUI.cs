using genshinbot.yui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
   public class ToolSelectorUI
    {
        private readonly Tab tab;
        private readonly YUI ui;

        public ToolSelectorUI(YUI ui, IEnumerable<ITool> tools)
        {
            this.ui = ui;
            tab = ui.CreateTab();
            tab.Title = "Tool selector";
            var content = tab.Content;
            content.SetFlex(new Flexbox { Direction = Orientation.Vertical, Scroll = true });
            var tv = content.CreateTreeview();
            content.SetFlex(tv, new Flexbox.Item { Weight = 1 });
            tv.BeginUpdate();
            foreach (var tool in tools)
            {
                if (tool is null) continue;
                var nd = tv.CreateNode();
                Type tt = tool.GetType();
                nd.Text = tt.Name;
                foreach (var meth in tt.GetMethods().Where(m => 
                m.GetParameters().Length == 0 && m.DeclaringType==tt
                
                ))
                {
                    var childe = nd.CreateChild();
                    childe.Text = meth.Name;
                    bool enabled = true;
                    childe.DoubleClick += async (_, _) =>
                    {
                        if (enabled)
                        {
                            enabled = false;
                            try
                            {
                                var res = meth.Invoke(tool, new object[] { });
                                if (res is Task t)
                                    await t;
                            }catch(Exception e)
                            {
                                tab.Status = e.ToString();
                                ui.GiveFocus(tab);
                            }
                            enabled = true;
                        }
                    };
                }
            }
            tv.EndUpdate();
        }
    }
}

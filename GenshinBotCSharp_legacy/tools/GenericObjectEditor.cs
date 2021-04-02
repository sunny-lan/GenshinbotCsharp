using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.tools
{
    class GenericObjectEditor
    {
        static YUI Ui;
        static void makeEditTab(object Db)
        {
            var editTab = Ui.CreateTab();
            var pg = editTab.Content.CreateTreeview();
            var root = pg.CreateNode();
            process(Db, root);
        }

        static void process( object o, yui.TreeView.Node nd)
        {
            if (o == null) return;
            var props = o.GetType().GetProperties();
            foreach (var prop in props)
            {
                var n = nd.CreateChild();
                n.Text = prop.Name + ": " + prop.PropertyType.Name;
                process(prop.GetValue( o), n);
            }
        }

        public static void Run()
        {
            Ui = yui.WindowsForms.MainForm.make();
            
        }
    }
}

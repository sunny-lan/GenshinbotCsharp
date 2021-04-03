﻿using System.Windows.Forms;

namespace genshinbot.yui.WindowsForms
{
    class Container : FlowLayoutPanel, yui.Container
    {
        public override bool AutoScroll => true;
        
        public void ClearChildren()
        {
            Invoke((MethodInvoker)delegate
            {
                Controls.Clear();
            });
        }

        public yui.Button CreateButton()
        {
            var btn = new Button();
            Invoke((MethodInvoker)delegate
            {
                btn.AutoSize = true;
                Controls.Add(btn);
            });
            return btn;
        }

        public yui.PropertyGrid CreatePropertyGrid()
        {
            var pg = new PropertyGrid();
            Invoke((MethodInvoker)delegate
            {
                Controls.Add(pg);
            });
            return pg;
        }

        public yui.Container CreateSubContainer()
        {
            var pg = new Container();
            Invoke((MethodInvoker)delegate
            {
                pg.AutoSize = true;
                Controls.Add(pg);
            });
            return pg;
        }

        public yui.TreeView CreateTreeview()
        {
            var t = new TreeView();
            Invoke((MethodInvoker)delegate
            {
                Controls.Add(t);
            });
            return t;
        }

        public yui.Viewport CreateViewport()
        {
            var vp = new Viewport();
            Invoke((MethodInvoker)delegate
            {
                Controls.Add(vp);
            });
            return vp;
        }

        public void Delete(object btn)
        {
            if (btn is Control c)
                Invoke((MethodInvoker)delegate
                {
                    Controls.Remove(c);
                });
            else Debug.Assert(false);
        }
    }
}

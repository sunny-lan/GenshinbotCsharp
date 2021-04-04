using System.Windows.Forms;

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

        private T add<T>(T c) where T : Control
        {
            Invoke((MethodInvoker)delegate
            {
                Controls.Add(c);
            });
            return c;
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

        public yui.Button CreateButton() => add(new Button());
        public yui.PropertyGrid CreatePropertyGrid() => add(new PropertyGrid());
        public yui.Slider CreateSlider() => add(new Slider());
        public yui.Container CreateSubContainer() => add(new Container());
        public yui.TreeView CreateTreeview() => add(new TreeView());
        public yui.Viewport CreateViewport() => add(new Viewport());

    }
}

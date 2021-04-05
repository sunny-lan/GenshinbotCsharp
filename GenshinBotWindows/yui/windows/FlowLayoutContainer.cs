using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class FlowLayoutContainer : FlowLayoutPanel, yui.Container
    {
        public FlowLayoutContainer():base()
        {
            BorderStyle = BorderStyle.FixedSingle;
        }

        void yui.Container.SetFlex(Flexbox layout)
        {
            AutoScroll = layout.Scroll;
            WrapContents = layout.Wrap;
            AutoSize = true;
            FlowDirection = layout.Direction == Orientation.Horizontal ? FlowDirection.LeftToRight : FlowDirection.TopDown;
        }

        void yui.Container.SetFlex(object child, Flexbox.Item layout)
        {

        }

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
        public yui.Slider CreateSlider() => add(new LabelledSlider());
        public yui.Container CreateSubContainer() => add(new FlowLayoutContainer());
        public yui.TreeView CreateTreeview() => add(new TreeView());
        public yui.Viewport CreateViewport() => add(new Viewport());

    }
}

using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
{
    class Container : FlowLayoutPanel, yui.Container
    {
        public yui.Button CreateButton()
        {
            var btn = new Button();
            Controls.Add(btn);
            return btn;
        }

        public yui.Viewport CreateViewport()
        {
            var vp = new Viewport();
            Controls.Add(vp);
            return vp;
        }
    }
}

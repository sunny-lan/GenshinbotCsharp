using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
{

    class Tab :TabPage, yui.Tab
    {
        Container _children;

        public Tab():base()
        {
            _children = new Container
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_children);
        }

        public string Title { get => Text; set => Text = value; }

        public yui.Container Content => _children;

        public Notifications Notifications => throw new System.NotImplementedException();
    }
}

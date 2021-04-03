using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
{

    class Tab : TabPage, yui.Tab
    {
        Container _children;

        public Tab() : base()
        {
            _children = new Container
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_children);
        }

        public string Title
        {
            get => Text;
            set => Invoke((MethodInvoker)delegate { Text = value; });
        }

        public yui.Container Content => _children;

        public string Status { get; set; }

        public Notifications Notifications => throw new System.NotImplementedException();
    }
}

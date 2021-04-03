using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
{

    class Tab : TabPage, yui.Tab
    {
        Container _children;
        private string status;
        

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

        public string Status
        {
            get => status; set
            {
                status = value;
                StatusChanged?.Invoke(value);
            }
        }
        internal Action<string> StatusChanged;

        public Notifications Notifications => throw new System.NotImplementedException();

    }
}

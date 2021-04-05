using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{

    class Tab : TabPage, yui.Tab
    {
        TablePanelContainer _children;
        private string status;
        

        public Tab() : base()
        {
            _children = new TablePanelContainer
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


    }
}

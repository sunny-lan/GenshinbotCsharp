using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class Dropdown : System.Windows.Forms.ComboBox, yui.Dropdown
    {
        private List<string> Options1=new();

        bool yui.Enablable.Enabled
        {
            get => base.Enabled;
            set => Invoke((MethodInvoker)delegate { Enabled = value; });
        }

        List<string> yui.Dropdown.Options
        {
            get => Options1;
            set
            {
                Options1 = value;
                Invoke((MethodInvoker)delegate {
                    this.Items.Clear();
                    this.Items.AddRange(Options1.ToArray());
                });
            }
        }

        public int Selected {
            get => base.SelectedIndex;
            set => Invoke((MethodInvoker)delegate { SelectedIndex = value; });
        }

        public event Action<int>? OptionSelected;

        public Dropdown()
        {

            base.SelectedIndexChanged += (_, _) => OptionSelected?.Invoke(SelectedIndex);
        }
    }
}

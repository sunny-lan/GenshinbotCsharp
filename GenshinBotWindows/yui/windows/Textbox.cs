using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class Textbox:System.Windows.Forms.TextBox,yui.Textbox
    {
        bool yui.Textbox.Enabled
        {
            get => base.Enabled;
            set => Invoke((MethodInvoker)delegate { Enabled = value; });
        }
        string yui.Textbox.Text
        {
            get => base.Text;
            set => Invoke((MethodInvoker)delegate { base.Text = value; });
        }

        public new event Action<string>? TextChanged;

        public Textbox()
        {

            base.TextChanged += (_, _) => TextChanged?.Invoke(Text);
        }
    }
}

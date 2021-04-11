using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class Label:System.Windows.Forms.Label,yui.Label
    {
        string yui.Label.Text
        {
            get => base.Text;
            set => Invoke((MethodInvoker)delegate { base.Text = value; });
        }
    }
}

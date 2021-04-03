﻿using System.Windows.Forms;

namespace genshinbot.yui.WindowsForms
{
    class Button : System.Windows.Forms.Button, yui.Button
    {
        bool yui.Button.Enabled
        {
            get => base.Enabled;
            set => Invoke((MethodInvoker)delegate { Enabled = value; });
        }
        string yui.Button.Text
        {
            get => base.Text;
            set => Invoke((MethodInvoker)delegate { Text = value; });
        }
    }
}

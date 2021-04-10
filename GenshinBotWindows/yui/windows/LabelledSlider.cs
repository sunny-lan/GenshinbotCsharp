using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    public partial class LabelledSlider : UserControl, yui.Slider
    {
        public LabelledSlider()
        {
            InitializeComponent();
        }
        bool yui.Slider.Enabled
        {
            get => trackBar.Enabled; set => Invoke((MethodInvoker)delegate
            {
                trackBar.Enabled = value;
            });
        }
        public int V
        {
            get => trackBar.Value; set => Invoke((MethodInvoker)delegate
            {
                trackBar.Value = value;
            });
        }
        public int Max
        {
            get => trackBar.Maximum; set => Invoke((MethodInvoker)delegate
            {
                trackBar.Maximum = value;
            });
        }
        public int Min
        {
            get => trackBar.Minimum; set => Invoke((MethodInvoker)delegate
            {
                trackBar.Minimum = value;
            });
        }


        string labelS;
        public string Label
        {
            get => labelS;
            set => Invoke((MethodInvoker)delegate
            {
                labelS = value;
                update();
            });
        }

        public event Action<int> VChanged;

        void update()
        {
            label.Text = labelS + ": " + V;
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            update();
            VChanged?.Invoke(trackBar.Value);
        }
    }
}

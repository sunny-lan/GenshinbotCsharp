using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot.yui.windows
{
    class Slider : TrackBar, yui.Slider
    {
        public int V { get => base.Value; set => base.Value = value; }
        public int Max { get => base.Maximum; set => base.Maximum=value; }
        public int Min { get => base.Minimum; set => base.Minimum=value; }
        public string Label { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event Action<int> VChanged;

        protected override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            VChanged?.Invoke(base.Value);
        }
    }
}

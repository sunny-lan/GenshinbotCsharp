using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace genshinbot
{
    static class Util2
    {
        public static Control FindFocusedControl(this Control control)
        {
            var container = control as IContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }
        public static Rect Cv(this Vanara.PInvoke.RECT r)
        {
            return new Rect(r.X, r.Y, r.Width, r.Height);
        }

    }
}

using genshinbot.automation.windows;
using genshinbot.diag;
using genshinbot.reactive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Vanara.PInvoke;

namespace genshinbot.yui.windows
{
    public partial class Overlay : Form
    {
        private ITestingRig rig;
        private Series arrowAng;

        public Overlay(ITestingRig r)
        {
            this.rig = r;
            InitializeComponent();
            IntPtr handle = this.Handle;

            User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_EXSTYLE,
              (IntPtr)((uint)User32.GetWindowLong(handle, User32.WindowLongFlags.GWL_EXSTYLE)
              | (uint)User32.WindowStylesEx.WS_EX_NOACTIVATE    ));

            Chart c = new Chart();
            c.Location = new Point(0, 0);
            c.Size = Size;

            ChartArea area = new ChartArea();
            area.Name = "default";
            area.AxisX.Title = "Time";
            area.AxisY.Maximum = 360;
            area.AxisY.Minimum = 0;

            arrowAng = new Series();
            arrowAng.Legend = "Arrow angle";
            arrowAng.ChartType = SeriesChartType.Line;
            arrowAng.ChartArea = "default";
            arrowAng.XValueType = ChartValueType.DateTime;
            arrowAng.YValueType = ChartValueType.Double;


            c.Series.Add(arrowAng);
            c.ChartAreas.Add(area);

            Controls.Add(c);
        }
        private void load()
        {
            var b = rig.Make();
            var p = new screens.PlayingScreen(b, null);
            


            Queue<Pkt<double>> q=new Queue<Pkt<double>>();
            var interval = TimeSpan.FromSeconds(10);
            p.ArrowDirection.Subscribe(x =>
            {
                if (x.Value == 0) return;
                q.Enqueue(x);
                while (q.Count > 0 && x.CaptureTime - q.Peek().CaptureTime > interval)
                    q.Dequeue();
                Invoke((MethodInvoker)delegate {
                    arrowAng.Points.Clear();
                    foreach (var x in q)
                    {
                        var deg = x.Value.ConfineAngle() * 180 / Math.PI;
                        arrowAng.Points.AddXY(x.CaptureTime, deg);
                    }
                });
            });
        }

        private void Overlay_Load(object sender, EventArgs e)
        {
            Task.Run(load);
        }
    }
}

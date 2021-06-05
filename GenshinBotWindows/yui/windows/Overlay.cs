using genshinbot.automation.windows;
using genshinbot.diag;
using genshinbot.reactive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
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
        private Chart chart;

        public Overlay(ITestingRig r)
        {
            this.rig = r;
            InitializeComponent();
            IntPtr handle = this.Handle;

            User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_EXSTYLE,
              (IntPtr)((uint)User32.GetWindowLong(handle, User32.WindowLongFlags.GWL_EXSTYLE)
              | (uint)User32.WindowStylesEx.WS_EX_NOACTIVATE));

            chart = new Chart();
            chart.Location = new Point(0, 0);
            chart.Size = Size;

            ChartArea area = new ChartArea();
            area.Name = "default";
            area.AxisX.Title = "Time";
            area.AxisY.Maximum = 360;
            area.AxisY.Minimum = 0;
            
            chart.ChartAreas.Add(area);

            Controls.Add(chart);

        }
        private void graph(IObservable<Pkt<double>> obs, string legend, AxisType axis=AxisType.Primary)
        {
            Series arrowAng;

            arrowAng = new Series();
            arrowAng.Legend = legend;
            arrowAng.ChartType = SeriesChartType.Line;
            arrowAng.ChartArea = "default";
            arrowAng.XValueType = ChartValueType.DateTime;
            arrowAng.YValueType = ChartValueType.Double;
            arrowAng.YAxisType = axis;
            Invoke((MethodInvoker)delegate
            {
                chart.Series.Add(arrowAng);
            });
            Queue<Pkt<double>> q = new Queue<Pkt<double>>();
            var interval = TimeSpan.FromSeconds(10);
            obs.Subscribe(x =>
            {
                if (x.Value == 0) return;
                q.Enqueue(x);
                while (q.Count > 0 && DateTime.Now - q.Peek().CaptureTime > interval)
                    q.Dequeue();
                Invoke((MethodInvoker)delegate
                {
                    arrowAng.Points.Clear();
                    foreach (var x in q)
                    {
                        var deg = x.Value.ConfineAngle() * 180 / Math.PI;
                        arrowAng.Points.AddXY(x.CaptureTime, deg);
                    }
                });
            });
        }
        private void load()
        {
            var b = rig.Make();
            var p = new screens.PlayingScreen(b, null);

            var wanted = Observable
                .FromEventPattern(
                    x => trackBar1.ValueChanged += x,
                    x => trackBar1.ValueChanged -= x
                )
                .Publish().RefCount()
                .Select(x => ((double)trackBar1.Value).Radians());
            var wantedPkt = wanted.Packetize();

            var alg = new algorithm.ArrowSteering(p.ArrowDirection, wanted);
            alg.MouseDelta.Subscribe(x => p.Io.M.MouseMove(new OpenCvSharp.Point2d(x, 0)));

            graph(p.ArrowDirection, "arrow dir");

        }

        private void Overlay_Load(object sender, EventArgs e)
        {
            Task.Run(load);
        }
    }
}

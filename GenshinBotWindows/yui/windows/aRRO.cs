using genshinbot.automation.windows;
using genshinbot.diag;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using genshinbot.util;
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
    public partial class aRRO : Form
    {
        private Chart chart;

        public static aRRO Instance=new aRRO();
        public ILiveWire<Pkt<double>> wantedPkt;

        public aRRO()
        {
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

            var wanted = new LiveWire<double>(
                () => Convert.ToDouble(
                    Invoke((Func<int>)(() => trackBar1.Value)))
                    .Normalize(trackBar1.Minimum,trackBar1.Maximum)
                , onChange =>
                {
                    void TrackBar1_ValueChanged(object? sender, EventArgs e)
                    {
                        onChange();
                    }
                    trackBar1.ValueChanged += TrackBar1_ValueChanged;
                    return DisposableUtil.From(() => trackBar1.ValueChanged -= TrackBar1_ValueChanged);
                });
            wantedPkt = wanted.Packetize();

        }
        public void graph(IWire<Pkt<double>> obs, string legend, AxisType axis=AxisType.Primary)
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
            var q = new Queue<Pkt<double>>();
            var interval = TimeSpan.FromSeconds(10);
            Pkt<double> ?last=null;
            void _graph(Pkt<double> x)

            {
                lock (q)
                {
                    if (last == null || last!.CaptureTime < x.CaptureTime)
                    {
                        q.Enqueue(x);
                        last = x;
                    }

                    while (q.Count > 0 && DateTime.Now - q.Peek().CaptureTime > interval)
                        q.Dequeue();

                    Invoke((MethodInvoker)delegate
                    {
                        arrowAng.Points.Clear();
                        foreach (var x in q)
                        {
                            arrowAng.Points.AddXY(x.CaptureTime, x.Value);
                        }
                    });
                }
            }
            if (obs is ILiveWire<Pkt<double>> ll)
                ll.Connect(_graph);
            else
                obs.Subscribe(_graph);
        }
        private void load()
        {
            var rig = new TestingRig();
            var b = rig.Make();
            var p = new screens.PlayingScreen(b, null);
            testSelect(p);
        }

        private void testSelect(screens.PlayingScreen p)
        {
            for (int _i = 0; _i < 4; _i++)
            {
                int i = _i;
                graph(p.PlayerSelect[i]
                    .Select(x => !x?i*360/4:(i+1)*360.0/4)
                    , $"player {i}");
            }
        }
        private void testHealth(screens.PlayingScreen p)
        {
            for(int _i = 0; _i < 4; _i++)
            {
                int i = _i;
                graph(p.PlayerHealth[i]
                    .Debug($"player {i}")
                    .Select(x => x.Denormalize(i*360.0/4, (i+1)*360.0/4))
                    .Debug($"player {i} ANLGE")
                    , $"player {i}");
            }
        }

        private void testArrow(screens.PlayingScreen p)
        {

            var wanted = this.wantedPkt.Select(x => x * Math.PI * 2);
            var alg = new algorithm.ArrowSteering(p.ArrowDirection, wanted.Depacket().AsNullable());
            alg.MouseDelta.Subscribe(x => p.Io.M.MouseMove(new OpenCvSharp.Point2d(x, 0)));



            graph(p.ArrowDirection.Select(x=> x.ConfineAngle().Degrees()), "arrow dir");
            graph(wanted.Select(x => x.ConfineAngle().Degrees()), "wanted");
        }        

        private void Overlay_Load(object sender, EventArgs e)
        {
            Task.Run(load);
        }
    }
}

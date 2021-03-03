using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinbotCsharp.gui
{
    public partial class WindowsGUI : Form
    {
        public WindowsGUI()
        {
            InitializeComponent();
        }

        Mat m=Data.Imread("test/map_a.png");
        int a = 0;

        private void button1_Click(object sender, EventArgs e)
        {

            pictureBox1.Image = MatBitmap.From(m);
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Re");
            a++;
            m.Circle(new OpenCvSharp.Point(a, a), 2, Scalar.Red, 2);
            pictureBox1.Invalidate();
        }
    }
}

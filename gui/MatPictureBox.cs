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
    public partial class MatPictureBox : PictureBox
    {
        private Mat _mat;
        private int a=0;
        public MatPictureBox()
        {
            InitializeComponent();
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.a += 10;
            _mat.Circle(new OpenCvSharp.Point(a, a), 2, Scalar.Beige, 2);
            Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _mat = Data.Imread("test/map_a.png");
            this.Image = MatBitmap.From(_mat);
            this.timer1.Start();

        }
    }
}

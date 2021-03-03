using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.algorithm
{
    class Padding
    {
       public Mat output = new Mat();
        public void Pad(Mat windowed, Size sz)
        {
            Cv2.CopyMakeBorder(windowed, output,
               0, sz.Height - windowed.Height,
               0, sz.Width - windowed.Width, BorderTypes.Constant);
        }
    }
}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.algorithm
{
    public class Hanning
    {
        Mat hanning = new Mat();

        ~Hanning()
        {
            hanning.Dispose();
        }

        /// <summary>
        /// Warning: applies on the original image!
        /// </summary>
        /// <param name="orig"></param>
        public void Window(Mat orig)
        {
            if (orig.Size() != hanning.Size())
            {
                Cv2.CreateHanningWindow(hanning, orig.Size(), orig.Type());
            }
            Cv2.Multiply(orig, hanning, orig);
        }

    }
}

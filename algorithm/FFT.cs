using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.algorithm
{
    class FFT
    {
        Mat zeroes=new Mat();
        Mat complex = new Mat();
        Mat magnitude = new Mat();
        public Mat output = new Mat();
       public void Run(Mat padded,int nonZeroRows=0)
        {

          

            if (zeroes.Size() != padded.Size())
            {
                zeroes = Mat.Zeros(padded.Size(), MatType.CV_32F);
            }

            Mat[] planes = { padded, zeroes };
            //TODO optimize this (no split then merge)
            Cv2.Merge(planes, complex);
            Cv2.Dft(complex, complex, nonzeroRows:nonZeroRows);
            planes = Cv2.Split(complex);
            Cv2.Magnitude(planes[1], planes[0], magnitude);
            magnitude /= magnitude.Width * magnitude.Height;
            magnitude.fftShift(output);
        }

    }
}

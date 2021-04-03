using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// the set of algorithms under algorithm.*
/// are designed to reuse memory when given images of the same size every time
/// however they still work if the image size changes
/// </summary>
namespace genshinbot.algorithm
{
   public class Padding
    {
       public Mat output = new Mat();

        ~Padding()
        {
            output.Dispose();
        }
        public void Pad(Mat windowed, Size sz)
        {
            Cv2.CopyMakeBorder(windowed, output,
               0, sz.Height - windowed.Height,
               0, sz.Width - windowed.Width, BorderTypes.Constant);
        }
    }
}

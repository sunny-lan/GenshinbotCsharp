using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.algorithm
{
    class StandardPhaseCorr
    {
       public class Input
        {
            Hanning window = new Hanning();
            Padding padding = new Padding();
            Mat cur;
            bool processed = false;

            public void Set(Mat m)
            {
                cur = m;
                processed = false;
            }

            public void Preprocess(Size padSz)
            {
                if (!processed)
                {
                    window.Window(cur);
                    padding.Pad(cur, padSz);
                    processed = true;
                }
            }

            public int Width => cur.Width;
            public int Height => cur.Height;

            public Mat output => padding.output;
        }

       public Input a=new Input(), b=new Input();
       static Mat empty = new Mat();
        public Point2d Run(out double response)
        {

            int pw = Cv2.GetOptimalDFTSize(Math.Max(a.Width, b.Width));
            int ph = Cv2.GetOptimalDFTSize(Math.Max(a.Height, b.Height));
            var sz = new Size(pw, ph);
            a.Preprocess(sz);
            b.Preprocess(sz);

            return Cv2.PhaseCorrelate(a.output, b.output, empty, out response);
        }
    }
}

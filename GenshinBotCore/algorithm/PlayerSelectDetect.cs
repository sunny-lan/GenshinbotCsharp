using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    public class PlayerSelectDetect
    {

        Mat numThres = new Mat(), hsvNum = new Mat(), satNum = new Mat();

        public bool ReadCharSelected(Mat m)
        {
            throw new NotImplementedException();
            /*var r = db.R[b.W.GetSize()];
            var rect = r.Characters[idx].Number;
            var pos = rect.Center().Round();
            var color = b.W.GetPixelColor(pos.X, pos.Y);
            var sMax = db.CharFilter.NumberSatMax.Expect();

            var hsv = color.CvtColor(ColorConversionCodes.BGR2HSV);
            return (hsv.Val1 > sMax);*/

        }
    }
}

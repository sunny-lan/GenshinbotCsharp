using genshinbot.data;
using OpenCvSharp;
using System.Diagnostics;

namespace genshinbot.algorithm
{
    public class NormComparer : SnapComparer
    {
        private Mat
           tmp = new Mat(),
           summed = new Mat(),
           tmpCvt = new Mat(),
            hsvSnap = new Mat()
           ;

        public NormComparer(Snap snap) : base(snap)
        {
            hsvSnap = snap.Image.Value;
            //Cv2.CvtColor(snap.Image.Value, hsvSnap, ColorConversionCodes.BGR2HSV);
        }

        public override double Compare(Mat screen)
        {
            Debug.Assert(screen.Size() == Snap.Image.Value.Size());
            tmpCvt = screen;
            //Cv2.CvtColor(screen, tmpCvt, ColorConversionCodes.BGR2HSV);
            /*Cv2.Absdiff(tmp, hsvSnap,tmp);
            tmp.ConvertTo(tmpCvt, MatType.CV_32FC3, 1 / 255.0);

            Cv2.Log(tmpCvt, tmpCvt);
            Cv2.Transform(tmpCvt, summed, new Scalar(1, 1, 1));
            Cv2.Pow(summed, 10, summed);

            Cv2.Add(summed, new Scalar(1, 1, 1, 1), summed);
            Cv2.Divide(new Scalar(1, 1, 1, 1), summed, summed);
            Cv2.Nor

            

            return Cv2.Sum(summed).SumComponents()/screen.Size().Area();*/
            return Cv2.Norm(tmpCvt, hsvSnap, NormTypes.L2SQR);
        }

        ~NormComparer()
        {
            tmp.Dispose();
        }
    }
}

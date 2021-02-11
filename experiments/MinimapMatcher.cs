using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace GenshinbotCsharp.algorithm.experiments
{
    static class MinimapMatcher
    {

        static Mat Recomb(Mat src)
        {
            int cx = src.Width >> 1;
            int cy = src.Height >> 1;
            Mat tmp = new Mat(src.Size(), src.Type());
            src[new Rect(0, 0, cx, cy)].CopyTo(tmp[new Rect(cx, cy, cx, cy)]);
            src[new Rect(cx, cy, cx, cy)].CopyTo(tmp[new Rect(0, 0, cx, cy)]);
            src[new Rect(cx, 0, cx, cy)].CopyTo(tmp[new Rect(0, cy, cx, cy)]);
            src[new Rect(0, cy, cx, cy)].CopyTo(tmp[new Rect(cx, 0, cx, cy)]);
            return tmp;
        }

        static Mat ForwardFFT(Mat src, bool do_recomb = true)
        {

            src.ConvertTo(src, MatType.CV_32F);
            Mat[] planes = { src, Mat.Zeros(src.Size(), MatType.CV_32F) };
            Mat complex = new Mat();
            Cv2.Merge(planes, complex);
            Cv2.Dft(complex, complex);
            planes = Cv2.Split(complex);
            if (do_recomb)
            {
                planes[0] = Recomb(planes[0]);
                planes[1] = Recomb(planes[1]);
            }
            for (int i = 0; i < 2; i++) planes[i] /= src.Width * src.Height;
            Mat res = new Mat();
            Cv2.Magnitude(planes[1], planes[0], res);
            return res;
        }

        static Mat InverseFFT(Mat[] FImg, bool do_recomb = true)
        {
            if (do_recomb)
            {
                for (int i = 0; i < 2; i++) FImg[i] = Recomb(FImg[i]);
            }
            Mat complexImg = new Mat();
            Cv2.Merge(FImg, complexImg);
            Mat result = new Mat();
            Cv2.Dft(complexImg, result, DftFlags.RealOutput);
            return result;
        }

        static Mat testfilter(Mat whole1, double param, double fac)
        {
            //var aa = new Mat();
            var aa = whole1.CvtColor(ColorConversionCodes.BGR2HSV);//.ConvertTo(aa, MatType.CV_32F, 1.0 / 255);

            //create windows of the stuff

            var ch = aa.Split();

            var r = ch[2];
            //Cv2.BitwiseXor(ch[1], r, r);
            //Cv2.BitwiseXor(ch[2], r, r);
            //Cv2.Multiply(ch[1], r, r);
            Mat r2 = new Mat();
            //Cv2.Laplacian(r, r2, MatType.CV_32F, ksize:3);
            // Cv2.Canny(r, r2, param, 255);
            //  Cv2.Threshold(ch[1], ch[1], 30, 255, ThresholdTypes.Binary);
            // Cv2.ImShow("re", ch[1]);
            //  Cv2.BitwiseAnd(ch[1], r2, r2);

            //Cv2.EqualizeHist(r, r2);
            Cv2.Laplacian(r, r2, MatType.CV_32F);
            r2.ConvertTo(r2, MatType.CV_32F, fac / 255.0);

            var hanning = new Mat();
            hanning.CreateHanningWindow(r2.Size(), MatType.CV_32F);
            Cv2.Multiply(r2, hanning, r2);

            var center = new Point(r2.Width / 2.0, r2.Height / 2.0);
            Cv2.Circle(r2, center, 30, Scalar.All(0), -1);

            //Cv2.Normalize(r2, r2,1,0,NormTypes.MinMax);

            return r2;
        }

        static void Test(Mat whole1, Mat sub1, out double angle, out double scale, out Point2d trans, int timeout=-1)
        {
            //extract value, convert to float
            var whole = testfilter(whole1, 255 / 1.3, 5);
            var sube = testfilter(sub1, 255 / 15.0, 3);
            //Cv2.ImShow("w", whole);
            //Cv2.ImShow("s", sub);

            //pad sub to size of whole in top left
            /* var sub = sube.CopyMakeBorder(0, whole.Height - sube.Height, 0, whole.Width - sube.Width, BorderTypes.Constant, Scalar.All(0));


             //take fft of both
             var wholeFft = ForwardFFT(whole, true);
             var subFft = ForwardFFT(sub, true);

             // wholeFft = wholeFft.Normalize(0, 1, NormTypes.MinMax);
             //  subFft = subFft.Normalize(0, 1, NormTypes.MinMax);

             //Cv2.ImShow("wfft", wholeFft * 255);
             //Cv2.ImShow("sfft", subFft * 255);

             //log polar transform on fft
             Mat wholePol = new Mat();
             Mat subPol = new Mat();
             Point2d center = new Point2d(wholeFft.Width / 2d, wholeFft.Height / 2d);
             double radius = Math.Min(center.X, center.Y);
             Size sz = new Size(wholeFft.Width, wholeFft.Height);
             Cv2.WarpPolar(wholeFft, wholePol, sz, center.ToPointf(), radius, InterpolationFlags.Linear, WarpPolarMode.Log);
             Cv2.WarpPolar(subFft, subPol, sz, center.ToPointf(), radius, InterpolationFlags.Linear, WarpPolarMode.Log);

            // Cv2.ImShow("wlg", wholePol * 255);
             //Cv2.ImShow("slg", subPol * 255);

             //phase correlate between transformed
             var res = Cv2.PhaseCorrelate(wholePol, subPol, new Mat(), out double response);
            // Console.WriteLine(res + " r=" + response);

             //convert resulting log polar coordinate back to normal
             angle = 360.0 * res.Y / wholePol.Height;
             double klog = wholePol.Width / Math.Log(radius);
             scale = Math.Exp(res.X / klog);*/


            //scale original image back and phase correlate
            scale = 1.18599262696709;
            angle = 0;
            Debug.show("rea", sube);
            var subFixed =  sube.Resize(default, fx:scale, fy:scale);// rotate(sube, angle, scale, out _);
            var ctr = subFixed.Center();
            Debug.show("rea", subFixed);
            subFixed = subFixed.CopyMakeBorder(0, 
                Max(whole.Height, subFixed.Height)-subFixed.Height, 0,
                 Max(whole.Width, subFixed.Width) - subFixed.Width, BorderTypes.Constant, Scalar.All(0));
            whole = whole.CopyMakeBorder(0,
                Max(whole.Height, subFixed.Height) - whole.Height, 0,
                 Max(whole.Width, subFixed.Width) - whole.Width, BorderTypes.Constant, Scalar.All(0));
            Debug.show("rea", subFixed);
            trans = Cv2.PhaseCorrelate(whole, subFixed, new Mat(), out var response1);
            Console.WriteLine("a="+angle+" s="+scale+" t="+trans + " r=" + response1);

            var center_trans =ctr-trans;
            var ans = whole1.Clone();
            ans.Circle(center_trans.ToPoint(), 2, Scalar.Red, 2);
            Debug.img = ans;
            Debug.show();
        }
        static Mat rotate(Mat src, double angle, double scale, out Mat matrix)
        {
            Mat dst = new Mat();
            var dsize = src.Size().Scale(scale);
            matrix = Cv2.GetRotationMatrix2D(dsize.Center().ToPointf(), angle, scale);
            Cv2.WarpAffine(src, dst, matrix, dsize);
            return dst;
        }

        public static void TestLive()
        {
            var sprinvile1 = new Rect(3790, 2005, 256, 256);
            var whole2 = Data.Imread("test/minimap_test_whole.png");
            var minimap1 = new Rect(53, 15, 189, 189);


            GenshinWindow g = new GenshinWindow();
            var r = g.GetRect();
            var buf = Screenshot.GetBuffer(r.Width, r.Height);
            while (true)
            {
                g.WaitForFocus().Wait();
                g.TakeScreenshot(0, 0, buf);
                var sub1 = buf.Mat;
                sub1 = sub1[minimap1].Resize(default, fx: 1.6, fy: 1.6, InterpolationFlags.Lanczos4);
                Debug.show("re", sub1);
                Test(whole2, sub1, out var angle, out var scale, out var trans,1);
            }
        }

        public static void Test()
        {
            TestLive();

            var sprinvile1 = new Rect(3790, 2005, 256, 256);
            var minimap1 = new Rect(46, 15, 189, 189);

            var whole1 = Data.Imread("map/genshiniodata/assets/MapExtracted_12.png");
            var whole2 = Data.Imread("test/minimap_test_whole.png");
            var sub1 = Data.Imread("test/minimap_test.png");
            sub1 = sub1[minimap1].Resize(default, fx: 1.6, fy: 1.6, InterpolationFlags.Lanczos4);
            sub1 = rotate(sub1, 20, 1, out _);
            sub1.Circle(sub1.Center().ToPoint(), 2, Scalar.AliceBlue, 2);
            Cv2.ImShow("re", sub1);
            Test(whole2, sub1, out var angle, out var scale, out var trans);
            


            var x = whole1[sprinvile1].Clone();
            x = x.Resize(default, fx: 1.1, fy: 1.1);
            //x = rotate(x, 20, 1.5);
           // Test(x, whole1[sprinvile1]);
        }
    }

}

using genshinbot.data;
using OpenCvSharp;
using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    public class ChatReadAlg
    {

        Mat filtered = new Mat();
     
        Mat img = new Mat();
        Mat hsv = new ();
            public void Read(Mat input)
        {
            var bac = new Scalar(255, 240, 30);
            Cv2.InRange(input, new (250, 250, 250), new (255, 255, 255), filtered);



            using var tesseract = OCRTesseract.Create("data/tessdata");
            tesseract.Run(filtered, out var txt, out var rects, out var texts, out var conf);
            Console.WriteLine(txt);
            Cv2.NamedWindow("r",WindowFlags.KeepRatio);
                Cv2.ImShow("r", filtered);
            Cv2.WaitKey();
        }

        public static void Test()
        {
            Mat m = Data.Imread("test/two_convos_f_1050.png");
            ChatReadAlg a = new();
            a.Read(m[Util.RectAround( new Point(900,300),new Point(1300,700))]);
        }
    }
}

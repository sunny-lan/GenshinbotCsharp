using OpenCvSharp;
using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace GenshinbotCsharp.algorithm
{
    class CharacterPaletteRead
    {
        private OCRTesseract tesseract;
        private TesseractEngine engine;

        public CharacterPaletteRead()
        {
           // this.tesseract = OCRTesseract.Create("data/tessdata",psmode:11);
           // this.engine = new Tesseract.TesseractEngine(datapath:"data/tessdata", language:"eng",configFile:"bazaar");
            //Console.WriteLine(engine.Version);
           // engine.SetVariable("language_model_penalty_non_freq_dict_word", "100");
           // engine.SetVariable("language_model_penalty_non_dict_word", "100");


        }
        public void Read(Mat palette)
        {

            var hsv = palette.CvtColor(ColorConversionCodes.BGR2HSV);
            var s = hsv.ExtractChannel(1);
            var v = hsv.ExtractChannel(2);

            Cv2.ImShow("S", s);
            Cv2.ImShow("v", v);
             var k = s.Threshold(10, 255, ThresholdTypes.BinaryInv);
            //var k = s.AdaptiveThreshold(255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 5, 10);
            Cv2.ImShow("ST", k);
            Mat m = v.CvtColor(ColorConversionCodes.GRAY2BGRA);
          /*  var bm = new Bitmap(m.Width, m.Height,
                (int)m.Step(),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                m.Data);


            // this.tesseract.Run(s, out var txt, out var _, out var _, out var _);
            using var pg = engine.Process(
                
             bm
                , PageSegMode.SparseText);

            Console.WriteLine(pg.GetText());*/
            Cv2.WaitKey();

        }

        public static void Test()
        {
            var a = Data.Imread("test/palette1.png");
            var b = Data.Imread("test/palette2.png");
            var cc = Data.Imread("test/moretext.png");
            CharacterPaletteRead c = new CharacterPaletteRead();
            while (true)
            {
                c.Read(a);
                c.Read(b);
                c.Read(cc);
            }
        }
    }
}

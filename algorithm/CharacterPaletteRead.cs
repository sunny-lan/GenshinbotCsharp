using OpenCvSharp;
using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.algorithm
{
    class CharacterPaletteRead
    {
        public class Db
        {
            public class RD
            {
                public class CharacterConfig
                {
                    public Rect Number { get; set; }
                }

                
            }

            public Dictionary<Size, RD> R { get; set; } = new Dictionary<Size, RD>
            {
            };
        }


        public CharacterPaletteRead()
        {
            
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

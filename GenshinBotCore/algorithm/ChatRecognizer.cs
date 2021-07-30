using FuzzySharp;
using genshinbot.data;
using OpenCvSharp;
using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static genshinbot.data.db.IconDb;

namespace genshinbot.algorithm
{

    public class ChatRecognizer
    {
        public record Result(
            string Text,
            Rect TextRect,
            double confidence,
            DialogueChoice MatchedText,
            double score,
            bool validMatch
        );
        static OCRTesseract tesseract = OCRTesseract.Create(
            datapath: Data.Get("tessdata"),
            charWhitelist: "qwertyuiopasdfghjklzxcvbnmPOIUYTREWQLKHGFDSAMNBVCXZ.? ",
            psmode: 4
        );
        public record DialogueChoice(string Value);
        

        Mat dif = new(), tmp = new();
        Mat tmp2 = new();
        Mat white = new();

        public float confThres = 40;
        public float minConf = 10;
        public int matchThres=50;

        public List<Result> Recognize(Mat m, DialogueChoice[] choices )
        {

            Cv2.BitwiseNot(m, dif);
            // Cv2.Absdiff(dColor, dif, dif);
            dif = dif.BilateralFilter(5, 200, 50);
            using var aa = dif.CvtColor(ColorConversionCodes.BGR2GRAY);
            using var bb = aa.CvtColor(ColorConversionCodes.GRAY2BGR);
            bb.ConvertTo(tmp, MatType.CV_64F, 1 / 255.0);
            //Cv2.Sqrt(tmp, tmp);
            dif.ConvertTo(tmp2, MatType.CV_64F);

            if (white.Size() != tmp2.Size())
            {
                white.Create(tmp2.Size(), tmp2.Type());
                white.SetTo(Scalar.White);
            }

            Cv2.Multiply(white, tmp, white);

            Cv2.Subtract(new Scalar(1, 1, 1), tmp, tmp);
            Cv2.Multiply(tmp2, tmp, tmp2);


            Cv2.Add(tmp2, white, tmp);
            tmp.ConvertTo(dif, MatType.CV_8U);
            tesseract.Run(
                dif,
                out string outputText,
                out Rect[] componentRects,
                out string?[] componentTexts,
                out float[] componentConfidence,
                ComponentLevels.TextLine);
            List<Result> res = new();
            for(int i = 0; i < componentRects.Length; i++)
            {
                if (componentTexts[i] is null) continue;
                string s = componentTexts[i]!.Trim();
                if (s.Length == 0) continue;
                if (componentConfidence[i] < minConf) continue;

                var res1=Process.ExtractOne(s, choices.Select(ch=>ch.Value));
                res.Add(new(
                    Text: componentTexts[i],
                    TextRect: componentRects[i],
                    confidence: componentConfidence[i],
                    MatchedText:choices[res1.Index],
                    score: res1.Score,
                    validMatch: res1.Score>matchThres && componentConfidence[i]>confThres
                    
                ));
            }
            return res;
        }
    }
}

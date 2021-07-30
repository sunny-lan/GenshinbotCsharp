using genshinbot.algorithm;
using genshinbot.data;
using OpenCvSharp;
using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GenshinBotTests
{
    public class ChatTest : MakeConsoleWork
    {
        public ChatTest(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void test2()
        {

            void dothing(Mat playingScreenImg, ChatRecognizer.DialogueChoice[] choices)
            {
                ChatRecognizer r = new();
                var res = (r.Recognize(playingScreenImg, choices));
                foreach (var thing in res)
                {
                    if (thing.validMatch)
                    {
                        Console.WriteLine($"{thing}");
                        playingScreenImg.Rectangle(thing.TextRect, Scalar.Red);
                    }
                }
                Console.WriteLine("---------");
            }
            dothing(Data.Imread($"test/Berry_crop.png"), new ChatRecognizer.DialogueChoice[]
               {
                    new("Berry")
               });
            dothing(Data.Imread($"test/convo_kath_isolated.png"), new ChatRecognizer.DialogueChoice[]
               {
                    new("Claim Daily Commision Rewards"),
                    new("Claim Adventure Rank Rewards"),
                    new("Dispatch Character on Expedition"),
                    new("Who are you?"),
                    new("Goodbye."),
               });
            dothing(Data.Imread($"test/f_max_crop.png"), new ChatRecognizer.DialogueChoice[]
               {
                    new("Bird Egg"),
                    new("Famed Handguard"),
                    new("Matsutake"),
                    new("Kageuchi Handguard"),
               });

        }

        [Fact]
        public void test()
        {

            void dothing(Mat playingScreenImg)
            {
                using var tesseract = OCRTesseract.Create(
                    datapath: Data.Get("tessdata"),
                    charWhitelist: "qwertyuiopasdfghjklzxcvbnmPOIUYTREWQLKHGFDSAMNBVCXZ.? ",
                    psmode: 4
                );
                var dColor = new Scalar(216, 229, 236);
                /*  var blob = SimpleBlobDetector.Create(new SimpleBlobDetector.Params
                  {
                      BlobColor= 230,


                  }); */
                Mat dif = playingScreenImg, tmp = new();
                Cv2.BitwiseNot(dif, dif);
                // Cv2.Absdiff(dColor, dif, dif);
                dif = dif.BilateralFilter(5, 200, 50);
                Cv2.EqualizeHist(dif, dif);

                //dif = dif.CvtColor(ColorConversionCodes.BGR2GRAY);
                // Cv2.InRange(tmp, new Scalar(0, 0, 0, 0), new Scalar(40, 40, 40, 255), dif);
                //.CvtColor(ColorConversionCodes.BGR2HSV);
                /*
    .ExtractChannel(1)
    .Threshold(20,255,ThresholdTypes.BinaryInv*/

                //Mat tmp = new();
                dif.CvtColor(ColorConversionCodes.BGR2GRAY).CvtColor(ColorConversionCodes.GRAY2BGR).ConvertTo(tmp, MatType.CV_64F, 1 / 255.0);
                //Cv2.Sqrt(tmp, tmp);
                Mat tmp2 = new();
                dif.ConvertTo(tmp2, MatType.CV_64F);

                Mat white = tmp2.Clone();
                white.SetTo(Scalar.White);

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
                Console.WriteLine(string.Join(",", componentTexts.Zip(componentConfidence).Select(x => (x.First ?? "-") + x.Second)));
                Console.WriteLine("-------");
            }
            dothing(Data.Imread($"test/Berry_crop.png"));
            dothing(Data.Imread($"test/convo_kath_isolated.png"));
            dothing(Data.Imread($"test/f_max_crop.png"));
            //dothing(Data.Imread($"test/"));
        }
    }
}

using genshinbot.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    class BlackWhiteTemplateMatchAlg
    {
        public Action<Mat, Mat> Preprocess { get; set; }
        public ColorRange PreprocessRange { get; set; } = new(new Scalar(255, 255, 255, 0), new Scalar(255, 255, 255, 255));
        public int TemplateBorder { get; set; } = 2;
        public double Threshold { get; set; } = 0.2;
        public BlackWhiteTemplateMatchAlg()
        {
            Preprocess = DefaultPreprocess;
        }

        public
            void DefaultPreprocess(Mat input, Mat output)
        {
            Cv2.InRange(input, PreprocessRange.Min, PreprocessRange.Max, output);
            Cv2.Sobel(output, output, MatType.CV_32F, 1, 1);//todo untested
        }

        Mat preprocessedPaimon = new Mat(), preScreen = new Mat(), paimonTemplMatch = new Mat(), derp = new Mat();

        ~BlackWhiteTemplateMatchAlg()
        {
            preprocessedPaimon.Dispose();
            preScreen.Dispose();
            paimonTemplMatch.Dispose();
            derp.Dispose();

        }

        public void SetTemplate(Mat t) 
        {
            Preprocess(t, preprocessedPaimon);
            Cv2.CopyMakeBorder(preprocessedPaimon, derp, 
                TemplateBorder, TemplateBorder, TemplateBorder, TemplateBorder,
             BorderTypes.Constant);

        }
        public (bool matched,double score) Match(Mat b)
        {
            Preprocess(b, preScreen);
            Cv2.MatchTemplate(preScreen, derp, paimonTemplMatch, TemplateMatchModes.SqDiffNormed);
            paimonTemplMatch.MinMaxLoc(out double res, out double _);
            return (res <= Threshold, res);

        }
    }
}

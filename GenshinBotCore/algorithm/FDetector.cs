using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    public class FDetector
    {
        class Filter
        {
            public readonly Mat v_sobel = new(),
                s_thres = new();
            private readonly Mat s = new(),
                v = new(),
                hsv = new();//todo dispose
            public readonly ConnectedComponents cc = new();
            public double SThresVal = 10;

            public void Apply(Mat input)
            {
                Cv2.CvtColor(input, hsv, ColorConversionCodes.BGR2HSV);
                Cv2.ExtractChannel(hsv, s, 1);
                Cv2.Threshold(s, s_thres, SThresVal, 255, ThresholdTypes.BinaryInv);
                cc.CalculateFrom(s_thres);
            }

            public Rect GetRect(int component,int padding)
            {
                return cc[component].Rect.Pad(padding).Intersect(hsv.ImgRect());
            }

            public void ApplySpecific(Rect rect)
            {
                Cv2.ExtractChannel(hsv[rect], v, 2);
                Cv2.Sobel(v, v_sobel, MatType.CV_32F, 1, 1);
            }
        }

        Filter templFilter = new(), inputFilter = new();
        Rect? templRect;

        public void SetTemplate(Mat template)
        {
            templFilter.Apply(template);
            Debug.Assert(templFilter.cc.Count == 2);
            templRect=templFilter.GetRect(component: 1, padding: 0);
            templFilter.ApplySpecific(templRect.Expect());
        }

        Mat templRes = new();
        public double matchThres=0.2;

        public (Rect r, double score)? Detect(Mat m)
        {
            var tRect = templRect.Expect();
            inputFilter.Apply(m);
            for (int comp = 1; comp < inputFilter.cc.Count; comp++)
            {
                var subRect=inputFilter.GetRect(component: comp, padding: 2);
                if (subRect.Width < tRect.Width || subRect.Height < tRect.Height)
                    continue;
                inputFilter.ApplySpecific(subRect);
                Cv2.MatchTemplate(inputFilter.v_sobel,
                    templFilter.v_sobel, templRes,
                    TemplateMatchModes.SqDiffNormed,
                    mask: templFilter.s_thres[tRect]);

                templRes.MinMaxLoc(out double score, out var _, out Point resLoc, out var _);
                if (score <= matchThres)
                {
                    return (new Rect(resLoc + subRect.TopLeft, tRect.Size), score);
                }

            }
            return null;
        }
    }
}

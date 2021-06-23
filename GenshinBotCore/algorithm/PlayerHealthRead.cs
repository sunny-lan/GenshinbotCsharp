using genshinbot.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    public class PlayerHealthRead
    {
        public class CharacterFilter
        {
            public double? NumberSatMax { get; set; }
            public ColorRange? HealthRed { get; set; }
            public ColorRange? HealthGreen { get; set; }
        }

        CharacterFilter CharFilter;
        double MinGreen = 0.3;

        public PlayerHealthRead(CharacterFilter charFilter)
        {
            CharFilter = charFilter;
        }

        Mat hsv1 = new Mat(), rthes1 = new Mat(), gthes1 = new Mat();

        /// <summary>
        /// Returns value from 0 to 1
        /// </summary>
        /// <param name="healthbar"></param>
        /// <returns></returns>
        public double ReadHealth(Mat healthbar)
        {
            var hr = CharFilter.HealthRed.Expect();
            var hg = CharFilter.HealthGreen.Expect();

            var rect = healthbar.Size();
            var src = healthbar;

            Cv2.CvtColor(src, hsv1, ColorConversionCodes.BGR2HSV);
            Cv2.InRange(hsv1, hg.Min, hg.Max, gthes1);
            var count = Cv2.CountNonZero(gthes1);
            double area = rect.Area();
            if (count >MinGreen * area) return count/area;

            Cv2.InRange(hsv1, hr.Min, hr.Max, rthes1);
            count = Cv2.CountNonZero(rthes1);
            return count / area;
        }

        ~PlayerHealthRead()
        {
            hsv1.Dispose();
            gthes1.Dispose();
            rthes1.Dispose();
        }
    }
}

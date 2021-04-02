using GenshinbotCsharp;
using GenshinbotCsharp.algorithm.MinimapMatch;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinBotWindows.algorithm.tests
{
    class MinimapMatchTest
    {
        public static void test()
        {
            Mat big = Data.Imread("map/genshiniodata/assets/MapExtracted_12.png");
            ScaleMatcher s = new ScaleMatcher(new Settings
            {
                BigMap = big,
            });

            Mat minimap = Data.Imread("test/minimap_test.png")[new Rect(46, 15, 189, 189)];

            var p = s.FindScale(new Point2d(3969, 2169), minimap, out var matcher);

            Console.WriteLine(p);
            Console.ReadKey();
        }
    }
}

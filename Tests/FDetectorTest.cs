using genshinbot;
using genshinbot.algorithm;
using genshinbot.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GenshinBotTests
{
    public class FDetectorTest:MakeConsoleWork
    {
        public FDetectorTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Test()
        {
            FDetector f = new ();
            Mat img = Data.Imread("test/f_katheryne_1050.png");
            Mat templ = img[new Rect(961, 508, 36, 33)];
            f.SetTemplate(templ);
            var res = f.Detect(img);
            Console.WriteLine(res);
            Assert.Equal(new Rect(x: 961, y: 509, width: 36, height: 29), res.Expect().r);
        }
    }
}

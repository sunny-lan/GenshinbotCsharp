using genshinbot.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GenshinBotTests
{
    public class ChatTest
    {
        [Fact]
        public void test()
        {
            Mat m = Data.Imread("test/two_convos_f_1050.png");
            Cv2.ImShow("hi",m);
        }
    }
}

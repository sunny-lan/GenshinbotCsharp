using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenshinbotCsharp.screens
{
    class PlayingScreen:Screen
    {
        private GenshinBot b;

        public PlayingScreen(GenshinBot b)
        {
            this.b = b;
        }

        public bool CheckActive()
        {
            throw new NotImplementedException();
        }

        public void OpenMap()
        {
            b.W.K.KeyPress(input.GenshinKeys.Map);
            Thread.Sleep(2000);//TODO
            b.S(b.MapScreen);
        }
        private Rect thing = new Rect(53, 15, 189, 189);//TODO
        private Rect thing_1440x900 = new Rect(46,13,161,161);//TODO

        private Screenshot.Buffer buf;
        public Mat SnapMinimap()
        {
            if(buf==null)
            {
                buf = Screenshot.GetBuffer(thing_1440x900.Width, thing_1440x900.Height);
            }
            b.W.TakeScreenshot(thing_1440x900.X, thing_1440x900.Y, buf);
            return buf.Mat;
        }
    }
}

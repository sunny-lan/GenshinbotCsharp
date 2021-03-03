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

        public MapScreen OpenMap()
        {
            b.W.K.KeyPress(input.GenshinKeys.Map);
            Thread.Sleep(2000);//TODO
            return b.S(b.MapScreen);
        }
        private Rect thing = new Rect(53, 15, 189, 189);//TODO

        private Screenshot.Buffer buf;
        public Mat SnapMinimap()
        {
            if(buf==null)
            {
                buf = Screenshot.GetBuffer(thing.Width, thing.Height);
            }
            b.W.TakeScreenshot(0, 0, buf);
            return buf.Mat;
        }
    }
}

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

        private int arrowRadius = 15;

        private static Dictionary<Size, Rect> miniMapLocs = new Dictionary<Size, Rect>
        {
            [new Size(1440,900)]= new Rect(46, 13, 161, 161),
            [new Size(1680,1050)]= new Rect(53, 15, 189, 189),
        };

        private Screenshot.Buffer buf;
        public Mat SnapMinimap()
        {
            var r = b.W.GetRect().Cv();
            var miniRect = miniMapLocs[r.Size];
            if (buf==null || buf.Size.cv()!=r.Size)
            {
                buf = Screenshot.GetBuffer(miniRect.Width, miniRect.Height);
            }
            b.W.TakeScreenshot(miniRect.X, miniRect.Y, buf);
            return buf.Mat;
        }

        private algorithm.ArrowDirectionDetect arrowDirection=new algorithm.ArrowDirectionDetect();

        private Screenshot.Buffer arrowBuf;
        Mat snapArrow()
        {
            var r = b.W.GetRect().Cv();
            var miniRect = miniMapLocs[r.Size];
            if (arrowBuf == null)
                arrowBuf = Screenshot.GetBuffer(arrowRadius*2, arrowRadius*2);
            var vec = (miniRect.Center() - new Point2d(arrowRadius, arrowRadius)).ToPoint();
            b.W.TakeScreenshot(vec.X, vec.Y, arrowBuf);
            return arrowBuf.Mat;
        }

        public double GetArrowDirection()
        {
            //make sure we are facing same direction as arrow
            //b.W.K.KeyPress(input.GenshinKeys.Forward);
            return arrowDirection.GetAngle(snapArrow());
        }

        public static void test()
        {
            GenshinBot b = new GenshinBot();
            var p = b.S(b.PlayingScreen);
            while (true)
            {
                Console.WriteLine(p.GetArrowDirection());
            }
        }

    }
}

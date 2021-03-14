using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenshinbotCsharp.screens
{
    class PlayingScreenDb
    {
        public class RD
        {
            public Rect minimapLoc { get; internal set; }
        }

        public Dictionary<Size, RD> R { get; set; } = new Dictionary<Size, RD>
        {
            [new Size(1440, 900)] = new RD
            {
                minimapLoc = new Rect(46, 13, 161, 161),

            },
            [new Size(1680, 1050)] = new RD
            {
                minimapLoc = new Rect(53, 15, 189, 189),
            },
        };
        public int arrowRadius { get; internal set; } = 15;
    }
    class PlayingScreen : Screen
    {
        private GenshinBot b;
        private PlayingScreenDb db = new PlayingScreenDb();//TODO

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

        private static Dictionary<Size, Rect> miniMapLocs = new Dictionary<Size, Rect>
        {
            [new Size(1440, 900)] = new Rect(46, 13, 161, 161),
            [new Size(1680, 1050)] = new Rect(53, 15, 189, 189),
        };

        public Mat SnapMinimap()
        {
            var miniRect = miniMapLocs[b.W.GetSize()];
            return b.W.TakeScreenshot(miniRect);
        }

        private algorithm.ArrowDirectionDetect arrowDirection = new algorithm.ArrowDirectionDetect();

        Mat snapArrow()
        {
            var miniRect = db.R[b.W.GetSize()].minimapLoc;
            return b.W.TakeScreenshot(miniRect.Center().RectAround(new Size(db.arrowRadius * 2, db.arrowRadius * 2)));
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

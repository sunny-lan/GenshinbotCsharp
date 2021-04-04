using genshinbot;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.screens
{
    public class PlayingScreen : Screen
    {
        public class Db
        {
            public class RD
            {
                public Rect MinimapLoc { get; internal set; }

                public class CharacterReadTemplate
                {
                    public int? HealthY { get; set; }

                    public int? NumberYBegin { get; set; }
                    public int? NumberYEnd { get; set; }

                    public int? NumberXBegin { get; set; }
                    public int? NumberXEnd { get; set; }

                    public int? HealthXBegin { get; set; }
                    public int? HealthXEnd { get; set; }
                }

                public CharacterReadTemplate CharTemplate { get; set; } = new CharacterReadTemplate();
                public int?[] TemplateYOffset { get; set; } = { 0, null, null, null };
            }

            public Dictionary<Size, RD> R { get; set; } = new Dictionary<Size, RD>
            {
                [new Size(1440, 900)] = new RD
                {
                    MinimapLoc = new Rect(46, 13, 161, 161),

                },
                [new Size(1680, 1050)] = new RD
                {
                    MinimapLoc = new Rect(53, 15, 189, 189),
                },
            };
            public int arrowRadius { get; internal set; } = 15;
        }

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
            b.K.KeyPress(input.GenshinKeys.Map);
            Thread.Sleep(2000);//TODO
            b.S(b.MapScreen);
        }

        public Mat SnapMinimap()
        {
            var db = b.Db.PlayingScreenDb;
            var miniRect = db.R[b.W.GetSize()].MinimapLoc;
            return b.W.Screenshot(miniRect);
        }

        private algorithm.ArrowDirectionDetect arrowDirection = new algorithm.ArrowDirectionDetect();

        Mat snapArrow()
        {
            var db = b.Db.PlayingScreenDb;
            var miniRect = db.R[b.W.GetSize()].MinimapLoc;
            return b.W.Screenshot(miniRect.Center().RectAround(new Size(db.arrowRadius * 2, db.arrowRadius * 2)));
        }

        public double GetArrowDirection()
        {
            //make sure we are facing same direction as arrow
            //b.K.KeyPress(input.GenshinKeys.Forward);
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

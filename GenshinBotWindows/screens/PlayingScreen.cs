using genshinbot;
using genshinbot.database;
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

                public class CharacterTemplate
                {
                    public Rect Health { get; set; }
                    public Rect Number { get; set; }
                }   

                public CharacterTemplate[] Characters { get; set; }
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
            public int ArrowRadius { get; internal set; } = 15;
            public int MinBlobArea { get; set; } = 20;
            public class CharacterFilter
            {
                public double? NumberSatMax { get; set; }
                public ColorRange? HealthRed{ get; set; }
                public ColorRange? HealthGreen{ get; set; }
            }

            public CharacterFilter CharFilter { get; set; } = new CharacterFilter();
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
            return b.W.Screenshot(miniRect.Center().RectAround(new Size(db.ArrowRadius * 2, db.ArrowRadius * 2)));
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

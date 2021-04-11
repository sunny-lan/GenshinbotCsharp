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

        Mat thresOut=new Mat(), hrThres=new Mat(), hgThres=new Mat(), hsvHealth=new Mat();

        /// <summary>
        /// Read health of player from side bar
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double ReadSideHealth(int idx)
        {
            var db = b.Db.PlayingScreenDb;
            var r = db.R[b.W.GetSize()];
            var rect = r.Characters[idx].Health;
            var src = b.W.Screenshot(rect);
            var hr = db.CharFilter.HealthRed.Expect();
            var hg = db.CharFilter.HealthGreen.Expect();

            Cv2.CvtColor(src, hsvHealth, ColorConversionCodes.BGR2HSV);

            //check green health
            Cv2.InRange(hsvHealth, hg.Min, hg.Max, hgThres); 
            var blob = Util.FindBiggestBlob(hgThres);
            if (blob != null )
            {
                return blob.Width / (double)rect.Width;
            }

            //check red health
            Cv2.InRange(hsvHealth, hr.Min, hr.Max, hrThres);
            blob = Util.FindBiggestBlob(hrThres);
            if(blob!=null )
            {
                return blob.Width / (double)rect.Width;
            }

            return 0;

        }


        Mat numThres=new Mat(),hsvNum=new Mat(), satNum=new Mat();

        public bool ReadCharSelected(int idx)
        {
            var db = b.Db.PlayingScreenDb;
            var r = db.R[b.W.GetSize()];
            var rect = r.Characters[idx].Number;
            var src = b.W.Screenshot(rect);
            var sMax = db.CharFilter.NumberSatMax.Expect();

            Cv2.CvtColor(src, hsvNum, ColorConversionCodes.BGR2HSV) ;
            Cv2.ExtractChannel(hsvNum, satNum, 1);
            Cv2.Threshold(satNum, numThres, sMax, 255, ThresholdTypes.BinaryInv);
            var blob = Util.FindBiggestBlob(numThres);
            return (blob.Area??0) < 0.7 * rect.Area();


        }
    }
}

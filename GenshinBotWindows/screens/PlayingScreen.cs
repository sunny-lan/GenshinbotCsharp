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
                public Rect MinimapLoc { get;  set; }

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
            public int ArrowRadius { get; set; } = 15;
            public int MinBlobArea { get; set; } = 20;
            public int MinAliveWidth { get; set; } = 10;
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

        Mat hsv1 = new Mat(), rthes1=new Mat(), gthes1 = new Mat();
        public bool ReadSideAlive(int idx)
        {
            var db = b.Db.PlayingScreenDb;
            var r = db.R[b.W.GetSize()];
            var rect = r.Characters[idx].Health;
            var hr = db.CharFilter.HealthRed.Expect();
            var hg = db.CharFilter.HealthGreen.Expect();

            rect.Width = db.MinAliveWidth;
            var src = b.W.Screenshot(rect);

            Cv2.CvtColor(src, hsv1, ColorConversionCodes.BGR2HSV);
            Cv2.InRange(hsv1, hg.Min, hg.Max, gthes1);
            var count = Cv2.CountNonZero(gthes1);
            if (count > 0.5 * rect.Area()) return true;

            Cv2.InRange(hsv1, hr.Min, hr.Max, rthes1);
            count = Cv2.CountNonZero(gthes1);
            if (count > 0.5 * rect.Area()) return true;
            return false;
        }

        Mat hrThres = new Mat(), hgThres = new Mat(), hsvHealth = new Mat();
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
        public double ReadSideHealth1(int idx)
        {
            var db = b.Db.PlayingScreenDb;
            var r = db.R[b.W.GetSize()];
            var rect = r.Characters[idx].Health;
            var my = (rect.Top + rect.Bottom) / 2;

            var hr = db.CharFilter.HealthRed.Expect();
            var hg = db.CharFilter.HealthGreen.Expect();


            int lo = rect.Left, hi = rect.Right;
            while (lo < hi)
            {
                int mx = (lo + hi) / 2;
                Scalar pixel = b.W.GetPixelColor(mx, my);
                Scalar hsv = pixel.CvtColor(ColorConversionCodes.BGR2HSV);

                if (hr.Contains(hsv) || hg.Contains(hsv))
                {
                    lo = mx+1;
                }
                else
                {
                    hi = mx;
                }
            }

            return lo;

        }

        Mat numThres=new Mat(),hsvNum=new Mat(), satNum=new Mat();

        public bool ReadCharSelected(int idx)
        {
            var db = b.Db.PlayingScreenDb;
            var r = db.R[b.W.GetSize()];
            var rect = r.Characters[idx].Number;
            var pos = rect.Center().Round();
            var color = b.W.GetPixelColor(pos.X, pos.Y);
            var sMax = db.CharFilter.NumberSatMax.Expect();

            var hsv = color.CvtColor(ColorConversionCodes.BGR2HSV);
            return (hsv.Val1 > sMax);

        }

        public bool IsDisabled()
        {
            for(int i = 0; i < 4; i++)
            {
                if (!ReadCharSelected(i) && ReadSideHealth(i)>0.1)
                {
                    return false;
                }
            }
            return true;
        }

        public static void TestRead()
        {
            GenshinBot b = new GenshinBot();
            b.InitDb();
            b.InitScreens();
            b.AttachWindow();
            
            while (true)
            {
                //System.Diagnostics.Debug.WriteLine(b.PlayingScreen.ReadCharSelected(0));
                System.Diagnostics.Debug.WriteLine(b.W.GetPixelColor(100,100));
            }
        }
    }
}

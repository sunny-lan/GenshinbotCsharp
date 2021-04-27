﻿using genshinbot.automation;
using genshinbot.automation.input;
using genshinbot.data;
using genshinbot.util;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace genshinbot.screens
{
    public class PlayingScreen
    {
        public class Db
        {
            public class RD
            {
                public Rect MinimapLoc { get; set; }

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
                public ColorRange? HealthRed { get; set; }
                public ColorRange? HealthGreen { get; set; }
            }

            public CharacterFilter CharFilter { get; set; } = new CharacterFilter();
        }

        private BotIO b;
        private Lazy<Db> _db = new Lazy<Db>(
            () => Data.ReadJson("screens/PlayingScreen.json", new Db()));
        public Db db => _db.Value;
        private IObservable<Db.RD> rd;
        public IObservable<Mat> Minimap { get; private init; }
        public IObservable<Mat> Arrow { get; private init; }
        public IObservable<double> ArrowDirection { get; private init; }

        public PlayingScreen(BotIO b)
        {
            this.b = b;
            rd = b.W.Size.Select(sz => db.R[sz]);
            Minimap = b.W.Screen.Watch(rd.Select(r => r.MinimapLoc));
            Arrow = b.W.Screen.Watch(
                rd.Select(r => r.MinimapLoc.Center().RectAround(new Size(db.ArrowRadius * 2, db.ArrowRadius * 2))));
            //TODO handle errors
            ArrowDirection = Arrow.Select(arrow => arrowDirectionAlg.GetAngle(arrow));
        }


        public async Task OpenMap()
        {
            await b.K.KeyPress(Keys.M);
            await Task.Delay(2000);//TODO
        }


        private algorithm.ArrowDirectionDetect arrowDirectionAlg = new algorithm.ArrowDirectionDetect();



        public static void test()
        {
            BotIO b = TestingRig.Make();
            var p = new PlayingScreen(b);
            using (p.ArrowDirection.Subscribe(
                onNext: x => Console.WriteLine($"angle={x}")
            ))
            {
                Console.ReadLine();
            }
        }

        public bool CheckActive()
        {
            throw new NotImplementedException();
        }

        /*

        Mat hsv1 = new Mat(), rthes1 = new Mat(), gthes1 = new Mat();
        public bool ReadSideAlive(int idx)
        {
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
            var r = db.R[b.W.GetSize()];
            var rect = r.Characters[idx].Health;
            var src = b.W.Screenshot(rect);
            var hr = db.CharFilter.HealthRed.Expect();
            var hg = db.CharFilter.HealthGreen.Expect();

            Cv2.CvtColor(src, hsvHealth, ColorConversionCodes.BGR2HSV);

            //check green health
            Cv2.InRange(hsvHealth, hg.Min, hg.Max, hgThres);
            var blob = Util.FindBiggestBlob(hgThres);
            if (blob != null)
            {
                return blob.Width / (double)rect.Width;
            }

            //check red health
            Cv2.InRange(hsvHealth, hr.Min, hr.Max, hrThres);
            blob = Util.FindBiggestBlob(hrThres);
            if (blob != null)
            {
                return blob.Width / (double)rect.Width;
            }

            return 0;

        }
        public double ReadSideHealth1(int idx)
        {
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
                    lo = mx + 1;
                }
                else
                {
                    hi = mx;
                }
            }

            return lo;

        }

        Mat numThres = new Mat(), hsvNum = new Mat(), satNum = new Mat();

        public bool ReadCharSelected(int idx)
        {
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
            for (int i = 0; i < 4; i++)
            {
                if (!ReadCharSelected(i) && ReadSideHealth(i) > 0.1)
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
                System.Diagnostics.Debug.WriteLine(b.W.GetPixelColor(100, 100));
            }
        }
        */
    }
}

using genshinbot.automation;
using genshinbot.automation.input;
using genshinbot.data;
using genshinbot.diag;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace genshinbot.screens
{
    public class PlayingScreen : IScreen
    {
        public class Db
        {
            public static Db Instance => inst.Value;
            private static Lazy<Db> inst = new Lazy<Db>(
                () => Data.ReadJson("screens/PlayingScreen.json", new Db()));
            public static async Task SaveInstanceAsync(Db instance = null)
            {
                if (instance == null) instance = Instance;
                await Data.WriteJsonAsync("screens/PlayingScreen.json", instance);
            }
            public class RD
            {
                public Rect MinimapLoc { get; set; }

                public class CharacterTemplate
                {
                    public Rect Health { get; set; }
                    public Rect Number { get; set; }
                }

                public CharacterTemplate[] Characters { get; set; } = new CharacterTemplate[4];

                public Snap ClimbingX { get; set; }
                public Snap FlyingSpace { get; set; }
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

            public double ClimbingXThreshold { get; set; } = 3000000;

            public algorithm.PlayerHealthRead.CharacterFilter CharFilter { get; set; }
                = new algorithm.PlayerHealthRead.CharacterFilter();
            public double MinAliveHealth { get; set; } = 0.02;
        }
        public Db db => Db.Instance;
        private ILiveWire<Db.RD?> rd;
        public IWire<Pkt<Mat>> Minimap { get; }
        public IWire<Pkt<Mat>> Arrow { get; }
        public IWire<Pkt<double>> ArrowDirection { get; }

        public IWire<Point2d> MinimapPos { get; }

        /*public enum TrackStatus
        {
            None,
            Tracking
        }
        public IWire<TrackStatus> MinimapTrackStatus=>trackStatus;
        private BehaviorSubject<TrackStatus> trackStatus=new BehaviorSubject<TrackStatus>(TrackStatus.None);*/

        class MinimapMatchSettingsAdapter : algorithm.MinimapMatch.Settings
        {
            public override Mat BigMap { get => Data.MapDb.BigMap.Load(); set => throw new NotImplementedException(); }
        }

        algorithm.MinimapMatch.ScaleMatcher scaleMatcher = new algorithm.MinimapMatch.ScaleMatcher(new MinimapMatchSettingsAdapter());

        /// <summary>
        /// it is up to the user to not call this concurrently
        /// It is expected as soon as a tracking error happens, 
        /// the wire returned will never be used again
        /// </summary>
        /// <param name="approxPos"></param>
        /// <returns></returns>
        public IWire<Pkt<Point2d>> TrackPos(Point2d approxPos,Action<Exception> onError)
        {

            algorithm.MinimapMatch.PositionTracker? posTrack = null;

            //TODO async
            return Minimap.Select((Mat x) =>
            {
            begin:
                Point2d res;
                if (posTrack == null)
                {
                    //scale not known yet

                    var res1 = scaleMatcher.FindScale(approxPos, x, out var posMatch);
                    if (res1 is Point2d r1)
                    {
                        res = r1;
                        posTrack = new algorithm.MinimapMatch.PositionTracker(posMatch);
                    }
                    else
                        throw new Exception("Failed to find valid scale");
                }

                var res2 = posTrack.Track(x);
                if (res2 is Point2d newApprox)
                {
                    res = newApprox;
                }
                else
                {
                    //unable to find position, check scale again
                    posTrack = null;
                    goto begin;
                }

                approxPos = res;
                return res;
            }
            //,    onError
            );
        }

        public IWire<Pkt<double>>[] PlayerHealth { get; } = new IWire<Pkt<double>>[4];
        public IWire<Pkt<bool>>[] PlayerSelect { get; } = new IWire<Pkt<bool>>[4];
        public IWire<Pkt<double>> ClimbingScoreX { get; }
        public IWire<Pkt<bool>> IsClimbing { get; }

        /// <summary>
        /// Not neccearily meaning dead
        /// Also is true when players are disabled (jumping, falling, climbing)
        /// </summary>
        public IWire<Pkt<bool>> IsAllDead { get; }

        public PlayingScreen(BotIO b, ScreenManager screenManager) : base(b, screenManager)
        {
            rd = b.W.Size.Select3(sz => db.R[sz]);
            Minimap = b.W.Screen.Watch2(rd.Select2(r => r.MinimapLoc));//TODO
            Arrow = b.W.Screen.Watch2(rd.Select2(r =>
                r.MinimapLoc.Center()
                .RectAround(new Size(db.ArrowRadius * 2, db.ArrowRadius * 2))
            ));
            //TODO handle errors+offload to separate thread!
            ArrowDirection = Arrow
                // .Debug("arrow IMG")
                .Select(
                    arrow =>
                    {
                        //  Console.WriteLine("begin detect");
                        var res = arrowDirectionAlg.GetAngle(arrow);
                        //  Console.WriteLine("end detect");
                        return res;
                    }
                    //,error => Console.WriteLine($"ERROR! {error}")
                )
                //.Debug("arrow DIR")
                ;

            for (int i = 0; i < PlayerHealth.Length; i++)
            {
                //we need to make 1 alg per player for memory thread safety
                var healthAlg = new algorithm.PlayerHealthRead(db.CharFilter);
                int cpy = i;
                PlayerHealth[i] = b.W.Screen
                    .Watch2(rd.Select2(rd => rd.Characters[cpy].Health))
                    .Select(healthAlg.ReadHealth);

                PlayerSelect[i] = b.W.Screen
                    .Watch2(rd.Select2(rd =>
                        //TODO sometimes false positive
                        //only check the single pixel in the middle, for efficiency
                        rd.Characters[cpy].Number.Center().RectAround(new Size(1,1))
                    ))
                    .Select(img =>
                    {
                        var color = img.Mean();
                        var sMax = db.CharFilter.NumberSatMax!;

                        var hsv = color.CvtColor(ColorConversionCodes.BGR2HSV);
                        return (hsv.Val1 > sMax);
                    });
                   
            }


            ClimbingScoreX = rd.Select3(rd =>
            {
                Debug.Assert(rd.ClimbingX is not null, 
                    $"ClimbingX is not configured in settings for size {b.W.Size.Value}");
                var comparer = new algorithm.NormComparer(rd.ClimbingX);
                return b.W.Screen.Watch(rd.ClimbingX.Region).Select(comparer.Compare);
            }).Switch2();
            IsClimbing = ClimbingScoreX.Select(x => x < db.ClimbingXThreshold);

            IsAllDead = PlayerHealth.Select((wire, idx) =>
                    wire.Select(
                        health =>
                        health < db.MinAliveHealth)
                   // .Debug($"p{idx}")
                )
                .ToArray()
                .AllLatest();//TODO not sure if debounce needed

            
        }


        public async Task OpenMap()
        {
            await Io.K.KeyPress(Keys.M);
            await ScreenManager.ExpectScreen(ScreenManager.MapScreen);

        }


        private algorithm.ArrowDirectionDetect arrowDirectionAlg = new algorithm.ArrowDirectionDetect();



        public static void test(ITestingRig rig)
        {
            BotIO b = rig.Make();
            var p = new PlayingScreen(b, null);
            using (p.ArrowDirection.Subscribe(
                x => Console.WriteLine($"angle={x}")
            ))
            {
                Console.ReadLine();
            }
        }

        public bool CheckActive()
        {
            throw new NotImplementedException();
        }

       
        public static async Task TestReadHealth(MockTestingRig rig1)
        {
            PlayingScreen p = new PlayingScreen(rig1.Make(), null);
            for (int i = 0; i < 4; i++)
                Console.WriteLine($"p[{i}] = {await p.PlayerHealth[i].Get()}");

        }
      
        public static void TestClimb2(ITestingRig rig1)
        {

            PlayingScreen p = new PlayingScreen(rig1.Make(), null);

            using (p.ClimbingScoreX.Subscribe(Console.WriteLine))
            {
                Console.ReadLine();
            }
        }
        /*
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
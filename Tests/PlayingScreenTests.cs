using genshinbot.data;
using genshinbot.diag;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using genshinbot.screens;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GenshinBotTests
{
    public class PlayingScreenTests : MakeConsoleWork
    {
        public PlayingScreenTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TestHealth()
        {


            async Task testAsync(string img, double[]? expRes = null)
            {
                var mt = Data.Imread($"test/{img}.png");
                var gw = new MockGenshinWindow(mt.Size());
                //gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");
                gw.CurrentScreen = gw.PlayingScreen;
                var rig1 = new MockTestingRig(gw);
                PlayingScreen p = new PlayingScreen(rig1.Make(), null);
                gw.PlayingScreen.Image = mt;
                Debug.WriteLine($"{img}:");
                for (int i = 0; i < 4; i++)
                {
                    var k = await p.PlayerHealth[i].Get();
                    Debug.WriteLine($" p[{i}]={Math.Round(k, 4)}:");
                    if (expRes is not null)
                        Assert.Equal(k, expRes[i], 3);
                }
            }
            await testAsync("guyun_playing_screen_1440x900", new[] { 0.06792452830188679, 0, 0, 0 });
            await testAsync("playing_luhua_1050");
            await testAsync("guyun_playing_climbing_1050");
            await testAsync("mondstadt_playing_climbing_1050");
            await testAsync("mondstadt_playing_climbing_2_1050");
            await testAsync("mondstadt_playing_walking_1050");
        }
        [Fact]
        public async Task TestMinimapTrack()
        {


            async Task testAsync(string img, Point2d approxPos, Point2d? pos = null)
            {
                var mt = Data.Imread($"test/{img}.png");
                var gw = new MockGenshinWindow(mt.Size());
                //gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");
                gw.CurrentScreen = gw.PlayingScreen;
                var rig1 = new MockTestingRig(gw);
                PlayingScreen p = new PlayingScreen(rig1.Make(), null);
                gw.PlayingScreen.Image = mt;
                var res = await p.TrackPos(approxPos).Get();
                Debug.WriteLine($"{img}, approx={approxPos} found={res}");
                double lim = 1;
                if (pos is Point2d pp)
                    Assert.True(res.DistanceTo(pp) < lim);
            }
            await testAsync("guyun_playing_screen_1440x900", new Point2d (4038,4361),
                new Point2d(x: 4037.29252119009, y: 4364.061027347406));
            await testAsync("playing_luhua_1050",new Point2d(2583,3879),
                new Point2d(x: 2572.27248046148, y: 3841.4867532292383));
            await testAsync("guyun_playing_climbing_1050", new Point2d(4038, 4361),
                new Point2d(x: 4035.7341975045856, y: 4388.821470157199));
            await testAsync("mondstadt_playing_climbing_1050",new Point2d(3875,1793),
                new Point2d(x: 3875.9510135143796, y: 1788.508902745817));
            await testAsync("mondstadt_playing_climbing_2_1050", new Point2d(3875, 1793),
                new Point2d(x: 3871.8887366283, y: 1788.4059563120397));
            await testAsync("mondstadt_playing_walking_1050", new Point2d(3875, 1793),
                new Point2d(x:3872.163712096001, y:1794.0877362238016));
        }




        [Fact]
        public async Task TestClimb()
        {
            var gw = new MockGenshinWindow(new Size(1680, 1050));
            //gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");


            gw.CurrentScreen = gw.PlayingScreen;

            var rig1 = new MockTestingRig(gw);
            PlayingScreen p = new PlayingScreen(rig1.Make(), null);

            async Task testAsync(string img, bool climbing)
            {
                gw.PlayingScreen.Image = Data.Imread($"test/{img}.png");
                var res = await p.ClimbingScoreX.Get();
                Debug.WriteLine($"{img} = {res}");
                var r2 = await p.IsClimbing.Get();
                Assert.Equal(r2, climbing);
            }

            await testAsync("playing_luhua_1050", false);
            await testAsync("guyun_playing_climbing_1050", true);
            await testAsync("mondstadt_playing_climbing_1050", true);
            await testAsync("mondstadt_playing_climbing_2_1050", true);
            await testAsync("mondstadt_playing_walking_1050", false);


        }

        [Fact]
        public async Task TestSelect()
        {
            var gw = new MockGenshinWindow(new Size(1680, 1050));
            //gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");


            gw.CurrentScreen = gw.PlayingScreen;

            var rig1 = new MockTestingRig(gw);
            PlayingScreen p = new PlayingScreen(rig1.Make(), null);

            async Task testAsync(string img, int? person)
            {
                gw.PlayingScreen.Image = Data.Imread($"test/{img}.png");
                Debug.WriteLine($"{img}:");
                int? res = null;
                for (int i = 0; i < 4; i++)
                {
                    var rr = await p.PlayerSelect[i].Depacket().Get();
                    Debug.WriteLine($"p[{i}]={(rr ? 't' : 'f')}");
                    if (rr)
                    {
                        Assert.True(res is null);
                        res = i;
                    }
                }
                Assert.Equal(res, person);
            }

            await testAsync("playing_luhua_1050", 1);
            await testAsync("guyun_playing_climbing_1050", 1);
            await testAsync("mondstadt_playing_climbing_1050", 1);
            await testAsync("mondstadt_playing_climbing_2_1050", 1);
            await testAsync("mondstadt_playing_walking_1050", 1);
            await testAsync("p1_selected_1050", 0);
            await testAsync("p4_selected_1050", 3);
            await testAsync("p2_selected_bright_1050", 1);


        }
        [Fact(Timeout = 5000)]
        public async Task TestAllDead()
        {
            var gw = new MockGenshinWindow(new Size(1680, 1050));
            //gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");


            gw.CurrentScreen = gw.PlayingScreen;

            var rig1 = new MockTestingRig(gw);
            PlayingScreen p = new PlayingScreen(rig1.Make(), null);

            async Task testAsync(string img, bool climbing)
            {
                gw.PlayingScreen.Image = Data.Imread($"test/{img}.png");
               bool r2;
                using (p.IsAllDead.Debug("isalldead").Use())
                {
                    await Task.Delay(100);//why so slow?
                    r2 = await p.IsAllDead.Get();
                }
                Debug.WriteLine($"{img} = {r2}");
                Assert.Equal(r2, climbing);
            }

            await testAsync("playing_luhua_1050", false);
            await testAsync("guyun_playing_climbing_1050", true);
            await testAsync("mondstadt_playing_climbing_1050", true);
            await testAsync("mondstadt_playing_climbing_2_1050", true);
            await testAsync("mondstadt_playing_walking_1050", false);


        }

        public async Task TestChat()
        {

        }
    }
}

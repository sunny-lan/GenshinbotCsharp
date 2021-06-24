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
    public class PlayingScreenTests
    {
        private readonly ITestOutputHelper Dbg;

        public PlayingScreenTests(ITestOutputHelper testOutputHelper)
        {
            Dbg = testOutputHelper;
        }
        [Fact]
        public  async Task TestHealth()
        {
            var gw = new MockGenshinWindow(new Size(1440, 900));
            //gw.MapScreen.Image = Data.Imread("test/map_luhua_1050.png");
            gw.PlayingScreen.Image = Data.Imread("test/guyun_playing_screen_1440x900.png");
            gw.CurrentScreen = gw.PlayingScreen;


            var rig1 = new MockTestingRig(gw);
            PlayingScreen p = new PlayingScreen(rig1.Make(), null);
            double[] expRes =
            {
                0.06792452830188679,0,0,0
            };
            for (int i = 0; i < 4; i++)
            {
                var k = await p.PlayerHealth[i].Get();
                Assert.Equal(k.Value, expRes[i], 3);
            }
        }

        

        [Fact]
        public  async Task TestClimb()
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
                Dbg.WriteLine($"{img} = {res}");
                var r2 = await p.IsClimbing.Get();
                Assert.Equal(r2.Value, climbing);
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
                Dbg.WriteLine($"{img}:");
                int ?res =null;
                for(int i = 0; i < 4; i++)
                {
                    var rr = await p.PlayerSelect[i].Depacket().Get();
                    Dbg.WriteLine($"p[{i}]={(rr?'t':'f')}");
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
            await testAsync("mondstadt_playing_walking_1050",1);
            await testAsync("p1_selected_1050", 0);
            await testAsync("p2_selected_bright_1050", 1);
            await testAsync("p4_selected_1050", 3);


        }
    }
}

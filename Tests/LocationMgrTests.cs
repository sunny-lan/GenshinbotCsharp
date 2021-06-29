using genshinbot;
using genshinbot.controllers;
using genshinbot.data;
using genshinbot.diag;
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
    public class LocationMgrTests : MakeConsoleWork
    {
        public LocationMgrTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TestMinimapTrack()
        {


            async Task testAsync(string playingImg, string mapImg, Point2d? pos = null)
            {
                var playingScreenImg = Data.Imread($"test/{playingImg}.png");
                var gw = new MockGenshinWindow(playingScreenImg.Size());
                gw.MapScreen.Image = Data.Imread($"test/{mapImg}.png");
                gw.PlayingScreen.Image = playingScreenImg;
                gw.CurrentScreen = gw.PlayingScreen;
                var rig1 = new MockTestingRig(gw);
                BotIO b = rig1.Make();

                ScreenManager mgr = new ScreenManager(b);
                mgr.ForceScreen(mgr.PlayingScreen);
                LocationManager lm = new LocationManager(mgr);
                var trackin = await lm.TrackPos(error=>Assert.True(false,error.ToString()));
                var res = await trackin.Get();

                Debug.WriteLine($"{playingImg},{mapImg}  found={res}");
                double lim = 2;
                if (pos is Point2d pp)
                    Assert.True(res.Value.DistanceTo(pp) < lim);
            }
            await testAsync("playing_luhua_1050", "map_luhua_1050",
                new Point2d(x: 942.1884150177244, y: 1177.772114829629));
        }

    }
}

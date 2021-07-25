using genshinbot;
using genshinbot.data;
using genshinbot.data.map;
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
    public class MapScreenTest : MakeConsoleWork
    {
        public MapScreenTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TestSelectorScan()
        {


            async Task testAsync(string img, Point?[]? expRes = null)
            {
                var mt = Data.Imread($"test/{img}.png");
                var gw = new MockGenshinWindow(mt.Size());
                gw.MapScreen.Image = mt;
                gw.CurrentScreen = gw.MapScreen;
                var rig1 = new MockTestingRig(gw);
                var ms = new MapScreen(rig1.Make(), null);
                FeatureType[] f = { FeatureType.Teleporter, FeatureType.Statue7, FeatureType.Domain };
                int idx = 0;
                foreach (var ff in f)
                {
                    var res = await ms.ScanForFeatureSelect(ff);
                    Console.WriteLine($" {ff}: {res}");
                    if (expRes is not null)
                    {
                        if (expRes[idx] is Point pp)
                            Assert.True(res.region.Expect().Contains(pp));
                        else
                            Assert.Null(res.region);
                    }
                    idx++;
                }
            }
            await testAsync("mapscreen_bunch_of_options_1050_1",
                new Point?[] { new(1134, 595), new(1134, 525), new(1134, 660) });
            await testAsync("map 1050 teleport and commisison",
                new Point?[] { new(1134, 812), null, null });
        }
        [Fact]
        public async Task TestTeleportButton()
        {


            async Task testAsync(string img, bool? expRes = null)
            {
                var mt = Data.Imread($"test/{img}.png");
                var gw = new MockGenshinWindow(mt.Size());
                gw.MapScreen.Image = mt;
                gw.CurrentScreen = gw.MapScreen;
                var rig1 = new MockTestingRig(gw);
                var ms = new MapScreen(rig1.Make(), null);
                var res = await ms.CheckIsTeleportButtonOpen();
                Console.WriteLine($"{img}: {res}");
                if (expRes is bool b)
                {
                    Assert.Equal(b, res.Open);
                }
            }
            await testAsync("mapscreen_bunch_of_options_1050_1", false);
            await testAsync("map 1050 teleport and commisison", false);
        }



    }
}

using genshinbot.automation;
using genshinbot.controllers;
using genshinbot.data;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using genshinbot.screens;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class WalkRecorder
    {
        private readonly LocationManager lm;
        private readonly ScreenManager mgr;

        public record Pt(
           Point2d Value,
           DateTime Time,
           double? Angle = null,
           double? Tolerance = null
           )
           : controllers.LocationManager.WalkPoint(Value, Tolerance);

        public WalkRecorder(LocationManager lm, ScreenManager mgr)
        {
            this.lm = lm;
            this.mgr = mgr;
        }
        public async Task<List<Pt>> RecordWalk()
        {
            mgr.ForceScreen(mgr.PlayingScreen);
            var trackin = await lm.TrackPos();
            Console.WriteLine("trakin begin");

            List<Pt> res = new();
            var pause = mgr.PlayingScreen.Io.W.KeyCap.KeyEvents
                    .Where(x => x.Down && x.Key == automation.input.Keys.OemSemicolon);
            Console.WriteLine("press ; to begin");
            await pause.Get().ConfigureAwait(false);
            Console.WriteLine("intializing...");
            Pkt<double>? dir = null;
            using (mgr.PlayingScreen.ArrowDirection.Subscribe(x => dir = x))
            {
                await mgr.PlayingScreen.ArrowDirection.Get().ConfigureAwait(false);
                using (trackin.Subscribe(
                   x => res.Add(new Pt(x.Value, x.CaptureTime, dir!.Value))
                ))
                {
                    await trackin.Get().ConfigureAwait(false);

                    Console.WriteLine("recording. press ; to stop");
                    await pause.Get().ConfigureAwait(false);
                }
            }
            Console.WriteLine("trakin end");
            return res;
        }

        public static async Task SaveWalkAsync(List<LocationManager.WalkPoint> walk, string name)
        {
            await Data.WriteJsonAsync($"walks/{name}.json", walk);
        }

        public static  List<LocationManager.WalkPoint>  LoadWalk( string name)
        {
            return  Data.ReadJson< List < LocationManager.WalkPoint > >($"walks/{name}.json");
        }
        public static async Task TestAsync(BotIO b)
        {
            ScreenManager mgr = new ScreenManager(b);
            mgr.ForceScreen(mgr.PlayingScreen);
            LocationManager lm = new LocationManager(mgr);
            var rec = new WalkRecorder(lm, mgr);
            while (true)
            {
                List<LocationManager.WalkPoint>? rr=null;
                if (Data.Exists("walks/tmp.json"))
                {

                    Console.WriteLine("previously recorded path exists. Press y to load");
                    if (Console.ReadKey().Key==ConsoleKey.Y)
                    {
                        rr = LoadWalk("tmp");
                    }
                }

                if(rr is null)
                {
                    Console.WriteLine("record path");
                    var r = await rec.RecordWalk();
                     rr = r.Cast<LocationManager.WalkPoint>().ToList();
                    await SaveWalkAsync(rr, "tmp");
                }
                Console.WriteLine("press enter to play path");
                Console.ReadLine();
                await lm.WalkTo(rr);
            }
        }
    }
}

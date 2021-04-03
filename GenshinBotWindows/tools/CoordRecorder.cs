using genshinbot.database.map;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    /// <summary>
    /// Scans map for waypoints and records their location
    /// </summary>
    class CoordRecorder
    {
        const string path = "map/db.json";
        public static void run(string[] args)
        {
            Console.WriteLine("clear old (y/n)?");
            var c = Console.ReadKey();

            MapDb db;
            if (c.Key != ConsoleKey.Y)
            {
                db = Data.ReadJson<MapDb>(path);

            }
            else
            {
                db = MapDb.Default();
            }

            var g = GenshinWindow.FindExisting();// ("*Untitled - Notepad", null);
            Console.WriteLine("genshin window initted");
            var features = db.Features;
            var m = new algorithm.MapTemplateMatch();
            var lm = new algorithm.MapLocationMatch(features);

            Console.WriteLine("Open map in genshin and focus onto it");
            g.WaitForFocus();
            var r = g.GetBounds();
            Console.WriteLine("data will be written automatically");


            while (true)
            {
                g.WaitForFocus();
                if (!g.Focused) break;
                var scr = g.Screenshot(r);


                var tr = m.FindTeleporters(scr).ToList();
                algorithm.MapLocationMatch.Result lr;
                try
                {
                    lr = lm.FindLocation2(tr, r.Size);
                }
                catch (algorithm.MapLocationMatch.NoSolutionException _)
                {
                    Console.WriteLine("failed to find location");
                    continue;
                }
                if (lr.Score != 0)
                {
                    Console.WriteLine("detect pos: " + lr.ToCoord(new Point2d(0, 0)) + " score=" + lr.Score);
                    for (int i = 0; i < features.Count; i++)
                    {
                        var f = features[i];
                        var p = lr.ToPoint(f.Coordinates).Round();
                        if (p.X > 0 && p.Y > 0 && p.X < r.Width && p.Y < r.Height)
                        {
                            Debug.img.PutText("f:" + i, p,
                                HersheyFonts.HersheyPlain, fontScale: 1, color: Scalar.Red, thickness: 2);
                        }
                    }
                }
                bool added = false;
                foreach (var match in lr.Matches)
                {
                    if (match.B == null)
                    {
                        var f = new Feature
                        {
                            Coordinates = lr.ToCoord(match.A.Point),
                        };
                        features.Add(f);
                        lm.AddFeature(f);
                        Console.WriteLine("new feature: " + f.Coordinates);
                        added = true;
                    }
                    else
                    {
                        int idx = features.IndexOf(match.B);
                        Debug.img.PutText("m:" + idx, match.A.BoundingBox.TopLeft, HersheyFonts.HersheyPlain,
                            fontScale: 1, color: Scalar.Cyan, thickness: 2);
                    }
                }
                Debug.show();
                if (added)
                    Data.WriteJson(path, db);

            }
            Console.WriteLine("points dump:");

            Console.WriteLine(features.Count);
            foreach (var p in features)
            {
                Console.WriteLine(p.Coordinates.X + " " + p.Coordinates.Y);
            }

            Console.ReadLine();
        }
    }
}

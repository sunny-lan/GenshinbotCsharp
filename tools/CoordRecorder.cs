using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.tools
{
    class CoordRecorder
    {
        static string path = Data.Get("map/points.txt");
        static List<Point2d> read()
        {
            if (!File.Exists(path))
                return new List<Point2d>();
            string[] lines = File.ReadAllLines(path);
            var r = new List<Point2d>(lines.Length);
            foreach(var line in lines)
            {
                var tmp = line.Split(' ');
                r.Add(new Point2d(
                    double.Parse(tmp[0]),
                    double.Parse(tmp[1])
                ));
            }
            return r;
        }

        static void write(List<Point2d> points)
        {
            string[] lines = points.Select(
                p => p.X + " " + p.Y
            ).ToArray();
            File.WriteAllLines(path, lines);
        }
        public static void run(string[] args)
        {
           /* Console.WriteLine("clear old (y/n)?");
            var c = Console.ReadKey();

            var points = new List<Point2d>();
            if (c.Key != ConsoleKey.Y)
            {
                points = read();
            }*/

            var g = new GenshinWindow();// ("*Untitled - Notepad", null);
            Console.WriteLine("genshin window initted");
            g.InitHooking();
            var m = new algorithm.MapTemplateMatch();
            var lm = new algorithm.MapLocationMatch();

            Console.WriteLine("Open map in genshin and focus onto it");
            g.WaitForFocus().Wait();
            var r = g.GetRect();
            var buf = Screenshot.GetBuffer(r.Width, r.Height);
            Console.WriteLine("move slowly");

            //position of current frame 
            var features = new List<Feature>();
            while (true)
            {
                g.WaitForFocus().Wait();
                if (!g.Focused) break;
                g.TakeScreenshot(0, 0, buf);


                var tr = m.FindTeleporters(buf.Mat).ToList();
                try
                {
                    var lr = lm.FindLocation2(tr, r.ToOpenCVRect().Size);
                    if (lr.Score != 0)
                    {
                        Console.WriteLine("detect pos: " + lr.ToCoord(new Point2d(0, 0)) + " score=" + lr.Score);
                        for (int i = 0; i < features.Count; i++)
                        {
                            var f = features[i];
                            Debug.img.PutText("f:" + i, lr.ToPoint(f.Coordinates).ToPoint(),
                                HersheyFonts.HersheyPlain, fontScale: 1, color: Scalar.Red, thickness: 2);
                        }
                        foreach (var match in lr.Matches)
                        {
                            if (match.B == null)
                            {

                            }
                            else
                            {
                                int idx = features.IndexOf(match.B);
                                Debug.img.PutText("m:" + idx, match.A.BoundingBox.TopLeft, HersheyFonts.HersheyPlain,
                                    fontScale: 1, color: Scalar.Cyan, thickness: 2);
                            }
                        }
                    }
                    foreach (var newPt in lr.Unknown)
                    {
                        var f = new Feature
                        {
                            Coordinates = lr.ToCoord(newPt.Point),
                        };
                        features.Add(f);
                        lm.AddFeature(f);
                        Console.WriteLine("new feature: " + f.Coordinates);
                    }
                }catch(Exception e) {
                    Console.WriteLine("failed to find location");
                }
                Debug.show();
                
            }
          /*  Console.WriteLine("points dump:");

            Console.WriteLine(points.Count);
            foreach(var p in points)
            {
                Console.WriteLine(p.X+ " " +p.Y);
            }
            write(points);*/
            Console.ReadLine();
        }
    }
}

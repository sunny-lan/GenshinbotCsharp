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
            Console.WriteLine("clear old (y/n)?");
            var c = Console.ReadKey();

            var points = new List<Point2d>();
            if (c.Key != ConsoleKey.Y)
            {
                points = read();
            }

            var g = new GenshinWindow();// ("*Untitled - Notepad", null);
            Console.WriteLine("genshin window initted");
            var m = new algorithm.MapFeatureMatch();

            Console.WriteLine("Open map in genshin and focus onto it");
            g.WaitForFocus().Wait();
            var r = g.GetRect();
            var buf = Screenshot.GetBuffer(r.Width, r.Height);
            Console.WriteLine("move slowly");

            //position of current frame 
            Point2d coordinate = new Point2d(0, 0);
            double THRES = 100; //expect not to move more than 100 pixels per frame
            while (true)
            {
                g.WaitForFocus().Wait();
                g.TakeScreenshot(0, 0, buf);

                //find all teleporters
                //match each teleporter to the closest one in the last frame
                //calculate the average offset of all teleporters
                var newPoints = new List<Point2d>();
                Point2d totalOffset = new Point2d(0, 0);
                int count = 0;
                foreach(var tel in m.FindTeleporters(buf.Mat))
                {
                    Point2d closest=default;
                    double min = double.PositiveInfinity;
                    foreach(var p in points)
                    {
                        var lastPos = p - coordinate;
                        double d = lastPos.DistanceTo(tel);
                        if (d < min)
                        {
                            min = d;
                            closest = p;
                        }
                    }

                    if (min > THRES) //new point
                    {
                        newPoints.Add(tel);
                    }
                    else
                    {
                        totalOffset += tel - closest;
                        count++;
                    }
                }

                if (count > 0) {
                    Point2d avgOffset = totalOffset * (1.0 / count);
                    coordinate += avgOffset;
                }
                else
                {
                    if (points.Count > 0)
                        throw new Exception("unable to track position");
                }
                Console.WriteLine("Coordinates: " + coordinate);

                //coordinate now holds the global position of the current frame
                //use this to add in all the new points
                foreach (var p in newPoints)
                {
                    points.Add(p+coordinate);
                    Console.WriteLine("New point: " + (p+coordinate));
                }
            }
            Console.WriteLine("points dump:");

            Console.WriteLine(points.Count);
            foreach(var p in points)
            {
                Console.WriteLine(p.X+ " " +p.Y);
            }
            write(points);
            Console.ReadLine();
        }
    }
}

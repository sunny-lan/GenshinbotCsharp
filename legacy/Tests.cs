using GenshinbotCsharp.database.map;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace GenshinbotCsharp.legacy
{
    class Tests
    {
        static void TestMap()
        {
            var m = new algorithm.MapTemplateMatch();
            var img = Data.Imread("test/mondstadt_default.PNG");
            foreach (var x in m.FindTeleporters(img))
            {
                Cv2.WaitKey();
            }
            Cv2.WaitKey();
        }

        static void TestMapLive()
        {
            var g = GenshinWindow.FindExisting();

            var m = new algorithm.MapTemplateMatch();

            while (true)
            {
                g.WaitForFocus();
                var r = g.GetRect();
                var b = Screenshot.GetBuffer(r.Width, r.Height);
                while (g.GetRect() == r)
                {
                    g.WaitForFocus();
                    g.TakeScreenshot(0, 0, b);
                    foreach (var x in m.FindTeleporters(b.Mat)) ;
                    Cv2.WaitKey(1);
                }
            }

        }

        static void TestLocationDetect()
        {

            var m = new algorithm.MapTemplateMatch();

            var a = Data.Imread("test/map_c.png");
            var b = Data.Imread("test/map_D.png");

            var features = m.FindTeleporters(a).Select(x => new Feature
            {
                Coordinates = x.Point,
            }).ToList();
            //features.Sort((x, y) => x.Coordinates.X.CompareTo( y.Coordinates.X));
            var lm = new algorithm.MapLocationMatch();
            foreach (var f in features)
                lm.AddFeature(f);

            Debug.show();


            Console.WriteLine("find teleporter");
            var tr = m.FindTeleporters(b).ToList();
            // tr.Sort((x, y) => x.Point.X.CompareTo(y.Point.X));
            Debug.show();
            Console.WriteLine("find loc");
            var lr = lm.FindLocation2(tr, b.Size());
            if (lr.Score != 0)
            {
                Console.WriteLine("detect pos: " + lr.ToCoord(new Point2d(0, 0)));
                for (int i = 0; i < features.Count; i++)
                {
                    var f = features[i];
                    Debug.img.PutText("f:" + i, lr.ToPoint(f.Coordinates).ToPoint(),
                        HersheyFonts.HersheyPlain, fontScale: 1, color: Scalar.Red, thickness: 2);
                }
                foreach (var match in lr.Matches)
                {
                    if (match.B != null)
                    {
                        int idx = features.IndexOf(match.B);
                        Debug.img.PutText("m:" + idx, match.A.BoundingBox.TopLeft, HersheyFonts.HersheyPlain,
                            fontScale: 1, color: Scalar.Cyan, thickness: 2);
                    }
                }
            }
            Debug.show();
            Debug.WaitKey();

        }


        static void testscreenshot()
        {
            var g = GenshinWindow.FindExisting();
            g.WaitForFocus();
            while (true)
            {
                var d = g.GetRect();
                Console.WriteLine(d.Size);
                var mt = Screenshot.GetBuffer(d.Width, d.Height);
                while (g.GetRect().Size == d.Size)
                {
                    if (g.Focused)
                    {
                        g.TakeScreenshot(0, 0, mt);
                        Cv2.BitwiseNot(mt.Mat, mt.Mat);
                    }
                    Cv2.ImShow("hi", mt.Mat);
                    Cv2.WaitKey(1);
                }
            }
        }
        static void TestRecord()
        {
            Recorder r = new Recorder();
            var b = new BetterHooker();

            Console.WriteLine("press esc to start");
            while (true)
            {
                var x = b.WaitEvent();
                if (x is KeyboardEvent e)
                {

                    if (e.KeyCode == VirtualKeyCode.ESCAPE)
                    {
                        if (e.KbType == KeyboardEvent.KbEvtType.DOWN)
                        {
                            if (r.Recording)
                            {
                                r.Stop();
                                Console.WriteLine("paused");
                            }
                            else
                            {
                                r.Start();
                                Console.WriteLine("recording...");
                            }
                        }
                    }
                    else if (e.KeyCode == VirtualKeyCode.END)
                    {
                        break;
                    }
                    else
                    {
                        r.OnEvent(x);
                    }

                }
            }


            Console.WriteLine("saving to recording.bin");
            using (BinaryWriter writer = new BinaryWriter(File.Open("recording.bin", FileMode.Create)))
            {
                r.Rec.To(writer);
            }
        }

        static void TestPlayback()
        {
            Console.WriteLine("press esc to play");
            var b = new BetterHooker();
            while (true)
            {
                var x = b.WaitEvent();
                if (x is KeyboardEvent e)
                    if (e.KbType == KeyboardEvent.KbEvtType.DOWN)
                        if (e.KeyCode == VirtualKeyCode.ESCAPE) break;
            }

            Task.Delay(1000).Wait();
            var iss = new WindowsInput.InputSimulator();
            using (var br = new BinaryReader(File.Open("recording.bin", FileMode.Open)))
            {
                Recording s = Recording.From(br);
                Player p = new Player(s);
                p.OnEvent += (_, e) =>
                {
                    if (e is KeyboardEvent x)
                    {
                        if (x.KbType == KeyboardEvent.KbEvtType.DOWN)
                        {
                            Console.WriteLine(x.KeyCode);
                            iss.Keyboard.KeyDown(x.KeyCode);
                        }
                        else
                        {
                            iss.Keyboard.KeyUp(x.KeyCode);
                        }
                    }
                };
                Console.WriteLine("playing...");
                p.Play();

                Console.WriteLine("done");
            }
            Console.ReadLine();
        }

    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace GenshinbotCsharp
{
    class Program
    {
        static void TestRecord()
        {
            Recorder r = new Recorder();
            using (var b = new BetterHooker())
            {
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
            using (var b = new BetterHooker())
            {
                while (true)
                {
                    var x = b.WaitEvent();
                    if (x is KeyboardEvent e)
                        if (e.KbType == KeyboardEvent.KbEvtType.DOWN)
                            if (e.KeyCode == VirtualKeyCode.ESCAPE) break;
                }

            }
            Task.Delay(1000).Wait();
            var iss= new WindowsInput.InputSimulator();
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


        static void Main(string[] args)
        {
            TestRecord();
             TestPlayback();
            
        }

    }
}

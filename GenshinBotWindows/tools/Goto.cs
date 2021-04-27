using genshinbot.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace genshinbot.tools
{
    class Goto
    {
        public static void Run()
        {
            Kernel32.AllocConsole();
            GenshinBot b = new GenshinBot();
            b.InitDb();
            b.AttachWindow();
            b.InitScreens();
            b.InitControllers();
            Mat big = Data.MapDb.BigMap.Load();
            Cv2.NamedWindow("select", WindowFlags.KeepRatio);
            while (true)
            {
                Console.WriteLine("Please navigate to play screen");
                //b.S(b.PlayingScreen);
                Rect zoom;
                while (true)
                {
                    zoom = Cv2.SelectROI("select", big);
                    if (zoom.Size.Width == 0) break;

                    var zoomed = big[zoom];
                    while (true)
                    {
                        Rect dst = Cv2.SelectROI("select", zoomed);
                        if (dst.Size.Width == 0 || dst.Size.Height == 0) break;

                        var d2 = Data.MapDb.Coord2Minimap.Expect().Inverse(dst.Center()+zoom.TopLeft);

                        b.W.TryFocus();
                     //TODO   b.LocationManager.WalkTo(d2, dst.Width);
                    }
                }
            }
        }
    }
}

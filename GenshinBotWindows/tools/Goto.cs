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
            Mat big = b.Db.MapDb.BigMap.Load();
            Cv2.NamedWindow("select", WindowFlags.KeepRatio);
            while (true)
            {
                Console.WriteLine("Please navigate to map screen");
                b.S(b.MapScreen);
                b.MapScreen.Close();
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

                        var d2 = b.Db.MapDb.Coord2Minimap.Expect().Inverse(dst.Center()+zoom.TopLeft);

                        b.W.TryFocus();
                        b.LocationManager.WalkTo(d2, dst.Width);
                    }
                }
            }
        }
    }
}

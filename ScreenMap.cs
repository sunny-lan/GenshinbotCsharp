using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace GenshinbotCsharp
{

    class ScreenMap
    {
        public static Dictionary<Size, ScreenMap> Configs = new Dictionary<Size, ScreenMap>
        {
            [new Size(1440, 900)] = new ScreenMap
            {
                Minimap = new MinimapMap
                {
                    Center = new Point(125, 92),
                    Radius = 91 - 12
                }
            }
        };
        public class MinimapMap
        {
            public Point Center;
            public int Radius;
        };
        public MinimapMap Minimap;

    }
}

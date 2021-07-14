using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data.db
{
    public class SettingsScreenDb
    {
        public readonly static DbInst<SettingsScreenDb> Instance = new("screens/settings.json");
        public class RD
        {
            public Rect Settings { get; set; }
            public Rect Graphics { get; set; }
            public Rect DisplayMode { get; set; }
            public Rect Windowed { get; set; }
            public Rect Fullscreen { get; set; }

        }

        public Dictionary<Size, RD> R { get; set; } = new();
    }
}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace GenshinbotCsharp
{
    class GenshinWindow : WindowAutomator
    {
        private GenshinWindow(string TITLE, string CLASS) : base(TITLE, CLASS)
        {
        }

        public static GenshinWindow FindExisting()
        {
            return new GenshinWindow("Genshin Impact", "UnityWndClass");
        }
    }
}

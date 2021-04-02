using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace GenshinbotCsharp
{
    public class GenshinWindow : WindowAutomator
    {
        public input.GenshinKeymap K;
        private GenshinWindow(string TITLE, string CLASS) : base(TITLE, CLASS)
        {
            K = new input.GenshinKeymap(I);
        }

        public static GenshinWindow FindExisting()
        {
            return new GenshinWindow("Genshin Impact", "UnityWndClass");
        }

    }
}

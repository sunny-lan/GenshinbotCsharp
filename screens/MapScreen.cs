using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace GenshinbotCsharp.screens
{
    class MapScreen
    {
        


        private GenshinWindow g;
        private Screenshot.Buffer buf;

        public MapScreen(GenshinWindow g)
        {
            this.g = g;
            var r = g.GetRect();
            buf = Screenshot.GetBuffer(r.Width, r.Height);

        }

       
        ~MapScreen()
        {
        }
    }
}

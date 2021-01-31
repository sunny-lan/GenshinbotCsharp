using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.screens
{
    class PlayingScreen
    {
        GenshinWindow g;
        public void Expect(GenshinWindow g)
        {
            //TODO actually check
            this.g = g;
        }

        public void Minimap(Screenshot.Buffer buf)
        {

        }
    }
}

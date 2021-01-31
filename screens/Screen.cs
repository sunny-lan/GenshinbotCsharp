using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.screens
{
    abstract class Screen
    {
        public abstract bool Is(GenshinWindow g);

    }
}

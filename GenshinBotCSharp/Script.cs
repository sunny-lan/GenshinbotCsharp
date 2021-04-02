using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    interface Script
    {
        void Load(GenshinBot b);
        void Unload(GenshinBot b);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot
{
    public interface Script
    {
        string DisplayName => this.GetType().FullName;
        void Load(GenshinBot b);
        void Unload(GenshinBot b);
    }
}

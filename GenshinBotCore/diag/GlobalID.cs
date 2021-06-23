using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.diag
{
    class GlobalID
    {
        static int g_id = 0;
        public static string get(string name="") =>$"{name}{Interlocked.Increment(ref g_id)}";
    }
}

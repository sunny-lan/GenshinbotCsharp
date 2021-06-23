using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.diag
{
    class ID
    {
        static int g_id = 0;
        public static int get() => Interlocked.Increment(ref g_id);
    }
}

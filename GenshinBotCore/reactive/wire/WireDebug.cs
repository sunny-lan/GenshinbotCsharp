using genshinbot.diag;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.reactive.wire
{
    public class WireDebug
    {
        public static WireDebug Instance = new WireDebug();
        public bool Enable=
            #if DEBUG 
            true
#else
            false
#endif
            ;

        public Info ? GetDebug()
        {
            if (Enable) return new Info();
            else return null;
        }

        public class Info
        {
            string here = Environment.StackTrace;
            string iid = GlobalID.get("wire");
        }
    }
}

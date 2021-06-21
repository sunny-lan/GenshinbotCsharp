using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.diag
{
    public static class Indent
    {
        static object lck = new object();
        //TODO locks
        static int indent = 0;
        public static IDisposable Inc(int num = 4)
        {
            lock (lck)
                indent += num;
            return
                Disposable
                .Create(() => { lock (lck) indent -= num; });
        }

        public static void Println(string s)
        {
            lock (lck)
                for (int i = 0; i < indent; i++)
                    Console.Write(' ');
            Console.WriteLine(s);
        }
    }
}

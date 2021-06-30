using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.util
{
    public static class ConsoleAsync
    {
        public static Task<string?> ReadLine()
        {
            return Task.Run(() => Console.ReadLine());
        }
    }
}

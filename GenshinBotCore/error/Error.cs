using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot
{
    class Error
    {
        public static void Throw<T>(T e) where T:Exception
        {
            throw e;
        }
    }
}

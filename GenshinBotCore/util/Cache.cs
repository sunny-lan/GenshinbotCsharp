using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.util
{
    class Cache<T>
    {
        public Cache(T init, Action calc)
        {
            actual = init;
            this.calc = calc;
        }
        public bool Valid { get; private set; }
        public T actual;
        private Action calc;

        public T V
        {
            get
            {
                if (!Valid) calc();
                Valid = true;
                return actual;
            }
        }

        public void Invalidate() { Valid = false; }
    }
}

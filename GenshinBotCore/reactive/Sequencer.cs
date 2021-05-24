using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.reactive
{
    public class Sequencer<T> : IDisposable
    {
        IObservable<T> o;
        private IDisposable dd;
        TaskCompletionSource<T> taskCompletionSource;

        public Sequencer(IObservable<T> o)
        {
            this.o = o;

           
        }

        public void Dispose() => dd.Dispose();
    }
}

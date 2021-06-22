using genshinbot.util;
using System;

namespace genshinbot.reactive.wire
{
    public class EmptyWire<T> : IWire<T>
    {
        
        public static EmptyWire<T> Instance=new EmptyWire<T>();

        private EmptyWire()
        {
        }

        public IDisposable Subscribe(Action<T> onValue)
        {
            return DisposableUtil.Empty;
        }
    }
}

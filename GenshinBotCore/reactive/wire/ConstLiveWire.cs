using genshinbot.util;
using System;

namespace genshinbot.reactive.wire
{
    public class ConstLiveWire<T>:ILiveWire<T>
    {
        private T val;

        public ConstLiveWire(T val)
        {
            this.val = val;
        }

        public T Value => val;

        public IDisposable Subscribe(Action<T> onValue)
        {
            return DisposableUtil.Empty;
        }
    }
}

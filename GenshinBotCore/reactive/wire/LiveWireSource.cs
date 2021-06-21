using genshinbot.util;
using System;

namespace genshinbot.reactive.wire
{
    public class LiveWireSource<T> : ILiveWire<T>
    {
        LiveWire<T> wire;
        private Action _onChange;
        T _v;
        public LiveWireSource(T v)
        {
            _v = v;
            wire = new LiveWire<T>(() => _v, onChange =>
            {
                this._onChange = onChange;
                return DisposableUtil.From(() => _onChange = null);
            });
        }

        public T Value => _v;

        public void Emit(T v)
        {
            _v = v;
            _onChange?.Invoke();
        }

        public IDisposable Subscribe(Action<T> onValue) => wire.Subscribe(onValue);
    }
}

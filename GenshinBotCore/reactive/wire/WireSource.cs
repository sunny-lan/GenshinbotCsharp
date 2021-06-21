using genshinbot.util;
using System;

namespace genshinbot.reactive.wire
{
    public class WireSource<T> : IWire<T>
    {
        Wire<T> wire;
        private Action<T> _onNext;

        public WireSource()
        {
            wire = new Wire<T>(onNext =>
            {
                this._onNext = onNext;
                return DisposableUtil.From(() => _onNext = null);
            });
        }

        public void Emit(T v) => _onNext?.Invoke(v);

        public IDisposable Subscribe(Action<T> onValue) => wire.Subscribe(onValue);
    }
}

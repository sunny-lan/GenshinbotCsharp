using genshinbot.util;
using System;

namespace genshinbot.reactive.wire
{
    public class WireSource<T> : IWire<T>
    {
        private bool _allowSallow;
        Wire<T> wire;
        private Action<T>? _onNext;

        public WireSource(bool allowSwallow=true)
        {
            _allowSallow = allowSwallow;
            wire = new Wire<T>(onNext =>
            {
                this._onNext = onNext;
                return DisposableUtil.From(() => _onNext = null);
            });
        }

        public void Emit(T v)
        {
            if (!_allowSallow && _onNext == null)
                throw new Exception("Value swallowed by WireSource!");
            _onNext?.Invoke(v);
        }

        public IDisposable Subscribe(Action<T> onValue,Action<Exception> e) => wire.Subscribe(onValue,e);

        public IDisposable Subscribe(Action<T> onValue) => wire.Subscribe(onValue);
    }
}

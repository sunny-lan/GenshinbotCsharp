using genshinbot.util;
using System;

namespace genshinbot.reactive.wire
{
    public class LiveWireSource<T> : ILiveWire<T>
    {
        LiveWire<T> wire;
        private Action? _onChange;
        private Action<Exception>? _eh;
        T _v;
        public LiveWireSource(T v)
        {
            _v = v;
            wire = new LiveWire<T>(() => _v, (onChange ,eh)=>
            {
                this._onChange = onChange;
                _eh = eh;

                return DisposableUtil.From(() => { _eh = null; _onChange = null; });
            });
        }


        public T Value => _v;

        public void SetValue(T v)
        {
            _v = v;
            _onChange?.Invoke();
        }

        public void EmitError(Exception e)
        {
            _eh?.Invoke(e);
        }

        public IDisposable Subscribe(Action<T> onValue) => wire.Subscribe(onValue);

        public IDisposable Subscribe(Action<T> onValue, Action<Exception> onErr) => wire.Subscribe(onValue, onErr);
    }
}

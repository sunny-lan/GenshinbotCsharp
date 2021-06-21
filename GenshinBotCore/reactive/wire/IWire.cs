using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace genshinbot.reactive.wire
{
    public interface IWire<out T>
    {
        IDisposable Subscribe(Action<T> onValue);
    }

    /// <summary>
    /// A wire, but always has a well defined value
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILiveWire<out T> : IWire<T>
    {
        public T Value { get; }


        /// <summary>
        /// Same as Subscribe, except calls onValue as soon as we are connected
        /// </summary>
        /// <param name="onValue"></param>
        /// <returns></returns>
        IDisposable Connect(Action<T> onValue)
        {
            onValue(Value);
            return Subscribe(onValue);
        }
    }
    public sealed class Void
    {
        public static Void V = new Void();
        private Void() { }
    }

    public class LiveWire<T> : Wire<T>, ILiveWire<T>
    {
        private T last;
        private bool running;
        private Func<T> getVal;

        public LiveWire(Func<T> getVal, Func<Action, IDisposable> enable) : base(
            onNext => enable(
                () => onNext(getVal())
            )
        )
        {
            this.getVal = getVal;
        }

        protected override void OnNext(T value)
        {
            if (running)
                //TODO inefficiency?
                if (EqualityComparer<T>.Default.Equals(last, value))
                    return;
            last = value;
            base.OnNext(value);
        }

        protected override void OnEnable(bool e)
        {
            running = e;
            if (e) last = getVal();
            else last = default;
            base.OnEnable(e);
        }

        //If its running, then just use the last value
        //Else need to call getVal()
        public T Value => running ? last : getVal();
    }
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
    public class Wire<T> : IWire<T>, IObservable<T>, IDisposable
    {
        List<Action<T>> subscribers = new List<Action<T>>();
        Func<Action<T>, IDisposable> Enable;

        public Wire(Func<Action<T>, IDisposable> enable)
        {
            Enable = enable;
        }

        IDisposable enabled;
        protected virtual void OnEnable(bool e)
        {
            if (e)
            {

                Debug.Assert(enabled == null);
                enabled = Enable.Invoke(OnNext);
            }
            else
            {

                Debug.Assert(enabled != null);
                enabled.Dispose();
                enabled = null;
            }
        }

        protected virtual void OnNext(T value)
        {
            Debug.Assert(enabled != null);
            List<Action<T>> cpy;
            lock (subscribers)
            {
                cpy = new List<Action<T>>(subscribers);
            }

            cpy.ForEach(subscriber => subscriber(value));
        }

        public IDisposable Subscribe(Action<T> onValue)
        {
            
            lock (subscribers)
                subscribers.Add(onValue);
            if (subscribers.Count == 1)
            {
                OnEnable(true);
            }
            return DisposableUtil.From(() =>
            {
                lock (subscribers)
                    subscribers.Remove(onValue);
                if (subscribers.Count == 0)
                {
                    OnEnable(false);
                }
            });
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return Subscribe(v => observer.OnNext(v));
        }

        public void Dispose()
        {
            //already disposed
            if (subscribers == null) return;
            subscribers = null;
            if (enabled != null)
                OnEnable(false);
        }

        ~Wire()
        {
            Dispose();
        }


    }
}

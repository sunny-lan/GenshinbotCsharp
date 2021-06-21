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
    public class Wire<T> : IWire<T>, IObservable<T>, IDisposable
    {
        List<Action<T>> subscribers = new List<Action<T>>();
        Func<Action<T>, IDisposable> Enable;
        object enabled_lock = new object();
        object S_lock = new object();
        //TODO performance optimization for single subscriber case

        public Wire(Func<Action<T>, IDisposable> enable)
        {
            Enable = enable;
        }

        IDisposable enabled;
        bool _enabled;
        protected virtual void OnEnable(bool e)
        {
            lock (enabled_lock)
            {

                if (e)
                {
                    Debug.Assert(!_enabled);
                    _enabled = true;
                    enabled = Enable.Invoke(OnNext);
                }
                else
                {

                    Debug.Assert(_enabled);
                    enabled.Dispose();
                    enabled = null;
                    _enabled = false;
                }
            }
        }

        protected virtual void OnNext(T value)
        {
            lock (enabled_lock)
            {
                Debug.Assert(_enabled);


                lock (S_lock)
                {
                    List<Exception> e = null;
                    foreach (var s in subscribers)
                    {
                        try
                        {
                            s(value);
                        }
                        catch (Exception ee)
                        {
                            if (e == null) e = new List<Exception>();
                            e.Add(ee);
                        }
                    }
                    if (e != null)
                        throw new AggregateException(e);
                }
            }
        }

        public IDisposable Subscribe(Action<T> onValue)
        {

            lock (S_lock)
            {
                subscribers = new List<Action<T>>(subscribers);
                subscribers.Add(onValue);
                if (subscribers.Count == 1)
                {
                    OnEnable(true);
                }
            }
            return DisposableUtil.From(() =>
            {
                lock (S_lock)
                {
                    subscribers = new List<Action<T>>(subscribers);
                    subscribers.Remove(onValue);
                    if (subscribers.Count == 0)
                    {
                        OnEnable(false);
                    }
                }
            });
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return Subscribe(v => observer.OnNext(v));
        }

        public void Dispose()
        {
            lock (S_lock)
            {
                Console.WriteLine($"Dispose Wire");
                //already disposed
                if (subscribers == null) return;
                subscribers = null;
                if (enabled != null)
                    OnEnable(false);
            }
        }

        ~Wire()
        {
            Dispose();
        }


    }
}

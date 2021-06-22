using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace genshinbot.reactive.wire
{
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

        volatile int grace = 0;
        protected virtual void OnNext(T value)
        {
            List<Action<T>> tmp;


            lock (S_lock)
            {
                if (_enabled) Interlocked.Exchange(ref grace, 0);
                else
                {
                    Debug.Assert(Interlocked.Increment(ref grace) < 2, "OnNext called when Wire disabled!!!");
                       
                }
                tmp = subscribers;
            }

            List<Exception> e = null;
            foreach (var s in tmp)
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

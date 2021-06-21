using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.reactive.wire
{
    class Disposable
    {
        class Impl : IDisposable
        {
            public Action dispose { get; init; }
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed) dispose();
            }

            ~Impl()
            {
                //TODO Dispose();
            }
        }
        public static IDisposable From(Action dispose)
        {
            return new Impl { dispose = dispose };
        }

        public static IDisposable Merge(params IDisposable[] dispose)
        {
            return new Impl
            {
                dispose = () =>
                {
                    foreach (var disposable in dispose)
                        disposable.Dispose();
                }
            };
        }
    }
    public interface IWire<T>
    {
        IDisposable Subscribe(Action<T> onValue);
    }

    /// <summary>
    /// A wire, but always has a well defined value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILiveWire<T> : IWire<T>
    {
        public T Value { get; }
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
            base.OnNext(value);
            last = value;
        }

        protected override void OnEnable(bool e)
        {
            base.OnEnable(e);
            running = e;
        }

        //If its running, then just use the last value
        //Else need to call getVal()
        public T Value => running ? last : getVal();
    }
    public class Wire<T> : IWire<T>, IObservable<T>,IDisposable
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
            foreach (var subscriber in subscribers)
                subscriber(value);
        }

        public IDisposable Subscribe(Action<T> onValue)
        {
            if (subscribers.Count == 0)
            {
                OnEnable(true);
            }

            subscribers.Add(onValue);
            return Disposable.From(() =>
            {
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
            if(enabled!=null)
            OnEnable(false);
        }

        ~Wire()
        {
            Dispose();
        }


    }


    public static class Wire
    {
        public static IObservable<T> AsObservable<T>(this IWire<T> t)
        {
            if (t is Wire<T> tt) return tt;
            return new Wire<T>(t.Subscribe);
        }
        public static async Task<T> Get<T>(this IWire<T> t)
        {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
            using (t.Subscribe(o =>
             {
                 taskCompletionSource.SetResult(o);
             }))
            {
                return await taskCompletionSource.Task;
            }
        }
        public static IWire<Out> Link<In, Out>(this IWire<In> w, Action<In, Action<Out>> f)
        {
            return new Wire<Out>(onNext =>
                  w.Subscribe(value => f(value, onNext))
            );
        }

        public static ILiveWire<Out> Select<In, Out>(this ILiveWire<In> w, Func<In, Out> f)
        {
            return new LiveWire<Out>(() => f(w.Value), onChange =>
                w.Subscribe(_ => onChange()));
        }


        public static IWire<Out> Select<In, Out>(this IWire<In> w, Func<In, Out> f)
        {
            return w.Link<In, Out>((value, next) => next(f(value)));
        }

        public static IWire<In> Where<In>(this IWire<In> w, Func<In, bool> f)
        {
            return w.Link<In, In>((value, next) =>
            {
                if (f(value)) next(value);
            });
        }
    }
}

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

    public interface IWireValue<T> : IWire<T>
    {
        public T Value { get; }
    }

    class Wire<T> : IWire<T>
    {
        List<Action<T>> subscribers = new List<Action<T>>();
        Func<Action<T>, IDisposable> Enable;

        public Wire(Func<Action<T>, IDisposable> enable)
        {
            Enable = enable;
        }

        IDisposable enabled;

        protected void OnNext(T value)
        {
            Debug.Assert(enabled != null);
            foreach (var subscriber in subscribers)
                subscriber(value);
        }

        public IDisposable Subscribe(Action<T> onValue)
        {
            if (subscribers.Count == 0)
            {
                Debug.Assert(enabled == null);
                enabled = Enable.Invoke(OnNext);
            }

            subscribers.Add(onValue);
            return Disposable.From(() =>
            {
                subscribers.Remove(onValue);
                if (subscribers.Count == 0)
                {
                    Debug.Assert(enabled != null);
                    enabled.Dispose();
                    enabled = null;
                }
            });
        }
    }

    public static class Wire
    {
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

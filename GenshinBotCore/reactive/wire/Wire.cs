using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

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
    public static class Wire
    {
        public static IWire<T> OnSubscribe<T>(this IWire<T> t, Func<IDisposable> f)
        {
            return new Wire<T>(onNext =>
            {
                return DisposableUtil.Merge(t.Subscribe(onNext), f());
            });
        }
        /// <summary>
        /// Fails a task if observable sends false while task isn't complete
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="t"></param>
        /// <param name="timeout">timeout on initial lock</param>
        /// <returns></returns>
        public static async Task LockWhile(this ILiveWire<bool> o, Func<Task> t, TimeSpan? timeout = null)
        {
            await o.WaitTrue(timeout);

            var taskCompletionSource = new TaskCompletionSource<Void>();

            using (o.Subscribe(
                x =>
                {
                    if (!x)
                        taskCompletionSource.SetException(
                            new LockInterruptedException("stream became false while running task"));
                }
            ))
            {
                var tt = await Task.WhenAny(t(), taskCompletionSource.Task);
                await tt;
            }
        }
        public static async Task Lock<T>(this IWire<T> o, Func<Task> t, T v)
        {
           

            var taskCompletionSource = new TaskCompletionSource<Void>();

            using (o.Subscribe(
                x =>
                {
                    if (!EqualityComparer<T>.Default.Equals(x,v))
                        taskCompletionSource.SetException(
                            new LockInterruptedException("value changed while running task"));
                }
            ))
            {
                var tt = await Task.WhenAny(t(), taskCompletionSource.Task);
                await tt;
            }
        }
        public static async Task<T> LockWhile<T>(this ILiveWire<bool> o, Func<Task<T>> t, TimeSpan? timeout = null)
        {
            await o.WaitTrue(timeout);

            var taskCompletionSource = new TaskCompletionSource<T>();

            using (o.Subscribe(
                x =>
                {
                    if (!x)
                        taskCompletionSource.SetException(
                            new Exception("stream became false while running task"));
                }
            ))
            {
                var tt = await Task.WhenAny(t(), taskCompletionSource.Task);
                return await tt;
            }
        }


        public static IWire<T> Switch<T>(this ILiveWire<IWire<T>> t)
        {
            var dist = t.DistinctUntilChanged();
            return new Wire<T>(onNext =>
            {
                IDisposable? last = null;
                var gen = dist.Connect(wire =>
                {
                    last?.Dispose();
                    last = wire.Subscribe(onNext);
                });
                return DisposableUtil.From(() =>
                {
                    gen.Dispose();
                    last?.Dispose();
                });
            });
        }


        public static ILiveWire<T> Switch<T>(this ILiveWire<ILiveWire<T>> t)
        {
            var dist = t.DistinctUntilChanged();
            return new LiveWire<T>(()=> dist.Value.Value,onChange =>
            {
                IDisposable? last = null;
                var gen = dist.Connect(wire =>
                {
                    last?.Dispose();
                    last = wire.Connect(_=>onChange());
                });
                return DisposableUtil.From(() =>
                {
                    gen.Dispose();
                    last?.Dispose();
                });
            });
        }
        public static IWire<T> Switch<T>(this IWire<IWire<T>> t)
        {
            var dist = t.DistinctUntilChanged();
            return new Wire<T>(onNext =>
            {
                IDisposable? last = null;
                var gen= dist.Subscribe(wire =>
                {
                    last?.Dispose();
                    last = wire.Subscribe(onNext);
                });
                return DisposableUtil.From(() =>
                {
                    gen.Dispose();
                    last?.Dispose();
                });
            }); 
        }

        
        public static ILiveWire<T> Combine<T,In1,In2>(ILiveWire<In1> t1, ILiveWire<In2> t2, 
            Func<In1,In2,T> f)
        {
            return new LiveWire<T>(() => f(t1.Value, t2.Value),
                onChange => 
                DisposableUtil.Merge(
                    t1.Subscribe(_ => onChange()),
                    t2.Subscribe(_ => onChange())

                ));
        }

        /// <summary>
        /// Returns the first value from live which isn't null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="w"></param>
        /// <returns></returns>
        public static async Task<T> Value2<T>(this ILiveWire<T?> w) where T:struct
        {
            if (w.Value is T t) return t;
            return await w.NonNull().Get();
        }

        public static IWire<T> NonNull<T>(this IWire<T?> w) where T : struct
        {
            return w.Link<T?, T>((x, next) =>
            {
                if (x is T t) next(t);
            });
        }


        public static IWire<T> Relay<T>(this IWire<T> t, ILiveWire<bool> control)
        {
            return control.Select(enable => enable ? t : EmptyWire<T>.Instance).Switch();
        }

        /// <summary>
        /// Returns a LiveWire which is null upon control=false, but retains the value of the original
        /// Livewire if true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        public static ILiveWire<T?> Relay2<T>(this ILiveWire<T?> t, ILiveWire<bool> control)
            where T:notnull
        {
            var empty = new ConstLiveWire<T?>(default);
            return control.Select<bool, ILiveWire<T?>>
                (enable => enable ? t  : empty).Switch();
        }
        public static ILiveWire<T?> AsNullable<T>(this ILiveWire<T> t)
         where T : struct
        {
            return t.Select(x => (T?)x);
        }

        //TODO ugly hack
        public static ILiveWire<T?> Relay3<T>(this ILiveWire<T> t, ILiveWire<bool> control)
            where T : struct
        {
            var empty = new ConstLiveWire<T?>(null);
            return control.Select<bool, ILiveWire<T?>>
                (enable=> 
                enable ? t.AsNullable() : empty
                ).Switch();
        }
        public static ILiveWire<T> DistinctUntilChanged<T>(this ILiveWire<T> t)
        {
            if(t is LiveWire<T> l)
            {
                if (l.ChecksDistinct) return l;//performance optimization
            }
            return new LiveWire<T>(()=>t.Value, onChange => t.Subscribe(_ => onChange()),true);
        }
        public static ILiveWire<T> ToLive<T, _>(this IWire<_> t, Func<T> get)
        {
            return new LiveWire<T>(get, onChange => t.Subscribe(_ => onChange()));
        }
        public static IObservable<T> AsObservable<T>(this IWire<T> t)
        {
            if (t is Wire<T> tt) return tt;
            return new Wire<T>(t.Subscribe);
        }
        
        public static async Task<T> Get<T>(this IWire<T> t, TimeSpan? timeout = null)
        {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
            using (t.Subscribe(o =>
             {
                 taskCompletionSource.SetResult(o);
             }))
            {
                if (timeout is TimeSpan tt)
                    _ = Task.Delay(tt).ContinueWith(_ => taskCompletionSource.SetException(
                          new TimeoutException()));
                return await taskCompletionSource.Task;
            }
        }

        public static async Task WaitTrue(this ILiveWire<bool> t, TimeSpan? timeout = null)
        {
            if (t.Value) return;
            while (!await t.Get(timeout)) ;
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


        public static ILiveWire<Out?> Select2<In, Out>(this ILiveWire<In?> w, Func<In, Out> f)
             where In:struct
            where Out:struct

        {
            Out? process()
            {
                if (w.Value is In i)
                    return f(i);
                return default;
            }
            return new LiveWire<Out?>(process, onChange =>
                w.Subscribe(_ => onChange()));
        }
        //TODO ugly hack to fix nullable checks
        public static ILiveWire<Out?> Select3<In, Out>(this ILiveWire<In?> w, Func<In, Out> f)
            where In : struct
           where Out : class

        {
            Out? process()
            {
                if (w.Value is In i)
                    return f(i);
                return default;
            }
            return new LiveWire<Out?>(process, onChange =>
                w.Subscribe(_ => onChange()));
        }
        public static ILiveWire<Out?> Select2<In, Out>(this ILiveWire<In?> w, Func<In, Out> f)
            where In : class
           where Out : struct

        {
            Out? process()
            {
                if (w.Value is In i)
                    return f(i);
                return default;
            }
            return new LiveWire<Out?>(process, onChange =>
                w.Subscribe(_ => onChange()));
        }


        public static IWire<T> DistinctUntilChanged<T>(this IWire<T> w)
        {
            bool first = true;
            T? last = default;
            return w.Where(x =>
            {
                if (first || !EqualityComparer<T>.Default.Equals(x, last))
                {
                    first = false;
                    last = x;
                    return true;
                }
                return false;
            });
        }
        public static IWire<T> Do<T>(this IWire<T> w, Action<T> f)
        {
            return w.Select(x => { f(x); return x; });
        }
        public static ILiveWire<T> Do<T>(this ILiveWire<T> w, Action<T> f)
        {
            return w.Select(x => { f(x); return x; });
        }
        public static IWire<Out> Select<In, Out>(this IWire<In> w, Func<In, Out> f)
        {
            return w.Link<In, Out>((value, next) => next(f(value)));
        }
        public static IWire<Out> ProcessAsync<In, Out>(this IWire<In> w, Func<In, Task<Out>> f)
        {
            //TODO swallows stuff
            return w.Link<In, Out>((value, next) => f(value).ContinueWith(t=>next(t.Result)));
        }
        public static IWire<Out> ProcessAsync<In, Out>(this IWire<In> w, Func<In, Out> f)
        {
            //TODO swallows stuff
            return w.Link<In, Out>((value, next) => Task.Run(()=>f(value)).ContinueWith(t => next(t.Result)));
        }
        public static IWire<In> Where<In>(this IWire<In> w, Func<In, bool> f)
        {
            return w.Link<In, In>((value, next) =>
            {
                if (f(value)) next(value);
            });
        }
        /// <summary>
        /// Returns a Livewire whose value is null when the function is null
        /// </summary>
        /// <typeparam name="In"></typeparam>
        /// <param name="w"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static ILiveWire<In?> Where2<In>(this ILiveWire<In> w, Func<In, bool> f) 
        {
            return w.Select<In,In?>((value) =>f(value)?value:default);
        }
        public static IWire<In> ToWire<In>(this IObservable<In> w)
        {
            return new Wire<In>(onNext => w.Subscribe(onNext));
        }
    }
}

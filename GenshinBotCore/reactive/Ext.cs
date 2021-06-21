using genshinbot.reactive;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace genshinbot
{
    namespace reactive
    {
        public static class Ext
        {
            /// <summary>
            /// runs async processing upon a stream, 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="U"></typeparam>
            /// <param name="o"></param>
            /// <param name="process"></param>
            /// <returns></returns>
            public static IObservable<U> ProcessAsync<T, U>(this IObservable<T> o, Func<T, Task<U>> process, int concurrentLimit = 1)
            {
                return o
                    .Select(x => Observable.FromAsync(() => process(x)))
                    .Merge(concurrentLimit);
            }
            public static IObservable<U> ProcessAsync<T, U>(this IObservable<T> o, Func<T, U> process, int concurrentLimit = 1)
            {
                return o.ProcessAsync(t => Task.Run(() => process(t)), concurrentLimit);
            }

            public static IObservable<U> Is<T, U>(this IObservable<T> o, U obj) where U : class where T : class
            {
                return o.Where(x => x == obj).Select(x => x as U);
            }
            public static IObservable<T> NonNull<T>(this IObservable<T> o) where T : class
            {
                return o.Where(x => x != null);
            }
            public static IObservable<T> NonNull<T>(this IObservable<T?> o) where T : struct
            {
                return o.Where(x => x != null).Expect("should never be null");
            }
            public static IObservable<T> Expect<T>(this IObservable<T?> o, string msg = "") where T : struct
            {
                return o.Select(x => x.Expect(msg));
            }
            /// <summary>
            /// Creates a new observable which is only subscribed to this when control=true
            /// Events sent before a signal on control will be ignored
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="o"></param>
            /// <param name="control"></param>
            /// <returns></returns>
            public static IObservable<T> Relay<T>(this IObservable<T> o, IObservable<bool> control)
            {
                var none = Observable.Never<T>();
                return control.Select(b => b ? o : none).Switch().Publish().RefCount();
            }
            public static Task WaitTrue(this IObservable<bool> oo, TimeSpan? timeout = null)
            {
                var o = oo
                    .Where(f => f);
                if (timeout is TimeSpan d)
                    o = o.Timeout(d);
                return o.Get();
            }

            /// <summary>
            /// Fails a task if observable sends false while task isn't complete
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="o"></param>
            /// <param name="t"></param>
            /// <param name="timeout">timeout on initial lock</param>
            /// <returns></returns>
            public static async Task LockWhile(this IObservable<bool> o, Func<Task> t, TimeSpan? timeout = null)
            {
                await o.WaitTrue(timeout);

                var taskCompletionSource = new TaskCompletionSource<Unit>();

                using (o.Subscribe(
                    onNext: x =>
                    {
                        if (!x)
                            taskCompletionSource.SetException(
                                new Exception("stream became false while running task"));
                    },
                    onCompleted: () => taskCompletionSource.SetException(
                        new Exception("stream ended while running task")),
                    onError: e => taskCompletionSource.SetException(
                        new Exception("error happened in stream while running task", e))
                ))
                {
                    var tt = await Task.WhenAny(t(), taskCompletionSource.Task);
                    await tt;
                }
            }
            public static  Task<T> LockWhile<T>(this IObservable<bool> o, Func<T> t, TimeSpan? timeout = null)


            {
                return o.LockWhile(() => Task.Run(t), timeout);
            }
            public static async Task<T> LockWhile<T>(this IObservable<bool> o, Func<Task<T>> t, TimeSpan? timeout = null)
            {
                await o.WaitTrue(timeout);

                var taskCompletionSource = new TaskCompletionSource<T>();

                using (o.Subscribe(
                    onNext: x =>
                    {
                        if (!x)
                            taskCompletionSource.SetException(
                                new Exception("stream became false while running task"));
                    },
                    onCompleted: () => taskCompletionSource.SetException(
                        new Exception("stream ended while running task")),
                    onError: e => taskCompletionSource.SetException(
                        new Exception("error happened in stream while running task", e))
                ))
                {
                    var tt = await Task.WhenAny(t(), taskCompletionSource.Task);
                    return await tt;
                }
            }

            public static async Task<T> Get<T>(this IObservable<T> o)
            {
                return await o.Take(1).ObserveOn(Scheduler.Default);
            }

            public static IObservableValue<Ret> CalculateFrom<Ret, Param1, Param2>(
                IObservableValue<Param1> a, IObservableValue<Param2> b,
                Func<Param1, Param2, Ret> combine
                )
            {
                var av = a.Value;
                var bv = b.Value;
                return new ObservableValue<Ret>(
                    Observable.Merge(
                        a.Select(nv => { av = nv; return (av: nv, bv: bv); }),
                        b.Select(nv => { bv = nv; return (av: av, bv: nv); })
                    )
                    .Select(nv => combine(nv.av, nv.bv)),
                    combine(a.Value, b.Value)
                );
            }
            public static IObservableValue<T> From<T>(this IObservable<T> o, T d)
            {
                return new ObservableValue<T>(
                     o,
                    d);
            }
            public static IObservableValue<T> Where<T>(this IObservableValue<T> o, Func<T, bool> f, T d)
            {
                return new ObservableValue<T>(
                     (o as IObservable<T>).Where(f),
                    f(o.Value) ? o.Value : d);
            }
            public static IObservableValue<T> DistinctUntilChanged<T>(this IObservableValue<T> o)
            {
                return new ObservableValue<T>(
                     (o as IObservable<T>).DistinctUntilChanged(),
                    o.Value);
            }

            public static IObservableValue<Output> Select<Input, Output>(this IObservableValue<Input> o, Func<Input, Output> f)
            {
                return new ObservableValue<Output>(
                    (o as IObservable<Input>).Select(f),
                    f(o.Value));
            }
        }
    }


}

﻿using genshinbot.util;
using OneOf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.reactive.wire
{

    public static partial class LiveWire
    {
        /// <summary>
        /// Same as Subscribe, except calls onValue as soon as we are connected
        /// TODO may double call
        /// </summary>
        /// <param name="onValue"></param>
        /// <returns></returns>
        public static IDisposable Connect<T>(this ILiveWire<T> t, Action<T> onValue)
        {
            var res = t.Subscribe(onValue);
            onValue(t.Value);
            return res;
        }
        public static IDisposable Connect<T>(this ILiveWire<T> t, Action<T> onValue, Action<Exception> eh)
        {
            var res = t.Subscribe(onValue, eh);
            onValue(t.Value);
            return res;
        }
        public static ILiveWire<T> Create<T>(T init, Func<Action<T>, IDisposable> enable)
        {
            T val = init;
            return new LiveWire<T>(() => val, onChange =>
               {
                   return enable(v =>
                   {
                       val = v;
                       onChange();
                   });
               });
        }
    }
    public static partial class Wire
    {
        /// <summary>
        /// Means that all dependencies must be subscribed to for this to work
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="w"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public static ILiveWire<U> DependsOn<U>(this ILiveWire<U> w, params IWire<object>[] dependency)
        {
            return w.OnSubscribe<U>(dependency
                .Select<IWire<object>, Func<IDisposable>>(d => d.Use)
                .ToArray());
        }
        public static IWire<U> DependsOn<U>(this IWire<U> w, params IWire<object>[] dependency)
        {
            return new Wire<U>((n, eh) =>
            {
                return DisposableUtil.Merge(
                    w.Subscribe(n, eh),
                    DisposableUtil.Merge(
                        dependency.Select<IWire<object>, IDisposable>(d =>
                    d.Subscribe(_ => { }, e => eh(e)))
                .ToArray()
                ));


            });
        }

        public static IWire<U> Limit<U>(this IWire<U> w, int cnt)
        {
            LiveWireSource<int> ctr = new(0);
            return ctr
                .Select(c => ctr.Value < cnt ? w : EmptyWire<U>.Instance)
                .Switch()
                .Do(_ => ctr.SetValue(ctr.Value + 1));
        }

        public static IWire<V> As<U, V>(this IWire<U> w) where U : V
        {
            return w.Select(x => (V)x);
        }


        public static IWire<Out> CombineLatest<In1, In2, In3, Out>(
            IWire<In1> t,
            IWire<In2> t2,
            IWire<In3> t3,
            Func<In1, In2, In3, Out> f
        )
        {
            return CombineLatest(t, CombineLatest(t2, t3, (t2, t3) => (t2, t3)), (t, a) =>
              {
                  return f(t, a.t2, a.t3);
              });
        }

        /// <summary>
        /// provids an efficient way to do x.select(x=>y.select()).switch()
        /// </summary>
        /// <typeparam name="In1"></typeparam>
        /// <typeparam name="In2"></typeparam>
        /// <typeparam name="Out"></typeparam>
        /// <param name="t"></param>
        /// <param name="t2"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static IWire<Out> CombineLatest<In1, In2, Out>(
            IWire<In1> t,
            IWire<In2> t2,
            Func<In1, In2, Out> f
        )
        {
            return new Wire<Out>((onNext, eH) =>
            {
                OneOf<In1, NoneT> p1 = NoneT.V;
                OneOf<In2, NoneT> p2 = NoneT.V;
                void onUpdate()
                {
                    if (p1.IsT0 && p2.IsT0)
                    {
                        onNext(f(p1.AsT0, p2.AsT0));
                    }
                }
                return DisposableUtil.Merge(
                    t.Subscribe(x => { p1 = x; onUpdate(); }, eH),
                    t2.Subscribe(x => { p2 = x; onUpdate(); }, eH)
                );
            });
        }
        /// <summary>
        /// Subscribe to wire without using the callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IDisposable Use<T>(this IWire<T> t)
        {
            return t.Subscribe(_ => { });
        }
        /// <summary>
        /// Calls function in infinite loop when the wire is suscribed
        /// </summary>
        /// <param name="a"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IWire<NoneT> InfiniteLoop(Action a, int? delay = null)
        {
            return new Wire<NoneT>((_, eH) => Poller.InfiniteLoop(a, delay, eH));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay">If null, goes in a fastest infinite loop</param>
        /// <returns></returns>
        public static IWire<NoneT> Interval(int? delay = null)
        {
            return new Wire<NoneT>((onNext, eH) => Poller.InfiniteLoop(() => onNext(NoneT.V), delay, eH));
        }
        public static IWire<T> OnSubscribe<T>(this IWire<T> t, params Func<IDisposable>[] f)
        {
            return new Wire<T>((onNext, eH) =>
            {
                return DisposableUtil.Merge(
                    t.Subscribe(onNext, eH),
                    DisposableUtil.Merge(f.Select(x => x()).ToArray())
                );
            });
        }
        public static ILiveWire<T> OnSubscribe<T>(this ILiveWire<T> t, params Func<IDisposable>[] f)
        {
            return new LiveWire<T>(() => t.Value, (onNext, eH) =>
              {
                  return DisposableUtil.Merge(
                      t.Subscribe(_ => onNext(), eH),
                      DisposableUtil.Merge(f.Select(x => x()).ToArray())
                  );
              });
        }


        public static async Task<U> Lock<V,U>(this ILiveWire<V> o,V v, Func<Task<U>> t)
        {
            if (!EqualityComparer<V>.Default.Equals(o.Value, v))
                throw new LockInterruptedException("value changed while running task");

            var taskCompletionSource = new TaskCompletionSource<U>();

            using (o.Subscribe(
                x =>
                {
                    if (!EqualityComparer<V>.Default.Equals(x, v))
                        taskCompletionSource.SetException(
                            new LockInterruptedException("value changed while running task"));
                }
            ))
            {
                var tt = await Task.WhenAny(t(), taskCompletionSource.Task).ConfigureAwait(false);
                return await tt.ConfigureAwait(false);
            }
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
            await o.WaitTrue(timeout).ConfigureAwait(false);

            var taskCompletionSource = new TaskCompletionSource<NoneT>();

            using (o.Subscribe(
                x =>
                {
                    if (!x)
                        taskCompletionSource.SetException(
                            new LockInterruptedException("stream became false while running task"));
                }
            ))
            {
                var tt = await Task.WhenAny(t(), taskCompletionSource.Task).ConfigureAwait(false);
                await tt.ConfigureAwait(false);
            }
        }
        public static bool LockWhile(this ILiveWire<bool> o, Action t, TimeSpan? timeout = null)
        {
            o.WaitTrue(timeout).Wait();

            bool res = true;
            using (o.Subscribe(
                x =>
                {
                    if (!x)
                        res = false;
                }
            ))
            {
                t();
            }
            return res;
        }
        public static async Task Lock<T>(this IWire<T> o, Func<Task> t, T v)
        {


            var taskCompletionSource = new TaskCompletionSource<NoneT>();

            using (o.Subscribe(
                x =>
                {
                    if (!EqualityComparer<T>.Default.Equals(x, v))
                        taskCompletionSource.SetException(
                            new LockInterruptedException("value changed while running task"));
                }
            ))
            {
                var tt = await Task.WhenAny(t(), taskCompletionSource.Task).ConfigureAwait(false);
                await tt.ConfigureAwait(false);
            }
        }
        public static async Task<T> LockWhile<T>(this ILiveWire<bool> o, Func<Task<T>> t, TimeSpan? timeout = null)
        {
            await o.WaitTrue(timeout).ConfigureAwait(false);

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
                var tt = await Task.WhenAny(t(), taskCompletionSource.Task).ConfigureAwait(false);
                return await tt.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Switches between multiple wires, if wire is unknown, defaults to Empty wire
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IWire<T> Switch2<T>(this ILiveWire<IWire<T>?> t, IWire<T>? defaultWire = null)
        {
            return t.Select(w =>
            {
                if (w is null) return defaultWire ?? EmptyWire<T>.Instance;
                else return w;
            }).Switch();
        }

        public static IWire<T> Switch<T>(this ILiveWire<IWire<T>> t)
        {
            var dist = t.DistinctUntilChanged();
            return new Wire<T>((onNext, eH) =>
            {
                IDisposable? last = null;
                var gen = dist.Connect(wire =>
                {
                    last?.Dispose();
                    last = wire.Subscribe(onNext, eH);
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
            return new LiveWire<T>(() => dist.Value.Value, (onChange, eh) =>
              {
                  IDisposable? last = null;
                  var gen = dist.Connect(wire =>
                  {
                      last?.Dispose();
                      last = wire.Connect(_ => onChange(), eh);
                  }, eh);
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
            return new Wire<T>((onNext, eH) =>
            {
                IDisposable? last = null;
                var gen = dist.Subscribe(wire =>
                 {
                     last?.Dispose();
                     last = wire.Subscribe(onNext, eH);
                 });
                return DisposableUtil.From(() =>
                {
                    gen.Dispose();
                    last?.Dispose();
                });
            });
        }

        public class CombineAsyncOptions
        {
            public bool Lock = false;
            /// <summary>
            /// Number of milliseconds to debounce
            /// null for no
            /// </summary>
            public int? Debounce;
        }
        public static CombineAsyncOptions DefaultCombineOptions = new CombineAsyncOptions();

        public static IWire<bool> AllLatest(this IWire<bool>[] t,
          CombineAsyncOptions? opt = null)
        {
            return CombineLatest(t, v => v.All(x => x), opt);
        }
        public static IWire<T> CombineLatest<T, In1>(this IWire<In1>[] t,
          Func<In1[], T> f, CombineAsyncOptions? opt = null)
        {
            int count = 0;
            In1?[] last = new In1?[t.Length];
            var thing = Wire.Merge(
                t.Select((wire, index) =>
                    wire.Do(val =>
                    {
                        if (last[index] == null) count++;
                        last[index] = val;
                    })
                )
            );

            object? lck = null;
            var opt2 = opt ?? Wire.DefaultCombineOptions;
            if (opt2.Lock) lck = new object();

            if (opt2.Debounce is int dd)
                thing = thing.Debounce(dd);

            return thing
                .Where(_ => count == t.Length)
                .Select(_ =>
                {
                    if (lck is null) return f(last!);
                    lock (lck) return f(last!);
                });
        }


        public static IWire<T> Merge<T>(this IEnumerable<IWire<T>> t)
        {
            return new Wire<T>((onNext, eH) =>
            {
                var dispose = t.Select(w => w.Subscribe(onNext, eH)).ToArray();
                return DisposableUtil.Merge(dispose);
            });
        }
        public static IWire<T> Debounce<T>(this IWire<T> t, int debounce)
        {
            long ctr = 0;
            return t.Link<T, T>((v, onChangeOld) =>
             {
                 long startCtr = Interlocked.Increment(ref ctr);
                 Task.Delay(debounce).ContinueWith(_ =>
                 {
                     var val = Thread.VolatileRead(ref ctr);
                     if (val == startCtr)
                         onChangeOld(v);
                 });
             });
        }

        public static ILiveWire<T> Debounce<T>(this ILiveWire<T> t, int debounce)
        {
             return new LiveWire<T>(() => t.Value, onChangeOld =>
               {
                   long ctr = 0;
                   Action onChange = () =>
                   {
                       long curCtr = Interlocked.Increment(ref ctr);
                       Task.Delay(debounce).ContinueWith(_ =>
                       {
                           if (ctr == curCtr)
                               onChangeOld();
                       });
                   };
                   return t.Subscribe(_ => onChange());
               });
        }

        public static ILiveWire<T> Combine<T, In1, In2>(ILiveWire<In1> t1, ILiveWire<In2> t2,
            Func<In1, In2, T> f, CombineAsyncOptions? opt = null)
        {
            object? lck = null;
            var opt2 = opt ?? DefaultCombineOptions;
            if (opt2.Lock) lck = new object();
            return new LiveWire<T>(() =>
            {
                if (opt2.Lock) lock (lck!) return f(t1.Value, t2.Value);
                return f(t1.Value, t2.Value);
            },
                (onChange, eh) =>
                {
                    if (opt2.Debounce is int debounce)
                    {
                        long ctr = 0;
                        var onChangeOld = onChange;
                        onChange = () =>
                        {
                            long curCtr = Interlocked.Increment(ref ctr);
                            Task.Delay(debounce).ContinueWith(_ =>
                            {
                                if (ctr == curCtr)
                                    onChangeOld();
                            });
                        };
                    }
                    return DisposableUtil.Merge(
                         t1.Subscribe(_ => onChange(), eh),
                         t2.Subscribe(_ => onChange(), eh)

                     );
                });
        }

        /// <summary>
        /// Returns the first value from live which isn't null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="w"></param>
        /// <returns></returns>
        public static async Task<T> Value2<T>(this ILiveWire<T?> w) where T : struct
        {
            if (w.Value is T t) return t;
            return await w.NonNull().Get().ConfigureAwait(false);
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
            where T : struct
        {
            var empty = new ConstLiveWire<T?>(default);
            return control.Select<bool, ILiveWire<T?>>
                (enable => enable ? t : empty).Switch();
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
                (enable =>
                enable ? t.AsNullable() : empty
                ).Switch();
        }
        public static ILiveWire<T> DistinctUntilChanged<T>(this ILiveWire<T> t)
        {
            if (t is LiveWire<T> l)
            {
                if (l.ChecksDistinct) return l;//performance optimization
            }
            return new LiveWire<T>(() => t.Value, (onChange, eh) => t.Subscribe(_ => onChange(), eh), true);
        }
        public static ILiveWire<T> ToLive<T, _>(this IWire<_> t, Func<T> get)
        {
            return new LiveWire<T>(get, (onChange, eh) => t.Subscribe(_ => onChange(), eh));
        }

        /// <summary>
        /// A live wire who 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ILiveWire<T> ToLive_WARNING<T>(this IWire<T> t, T intial)
        {
            return new LiveWireSource<T>(intial);
        }
        public static IObservable<T> AsObservable<T>(this IWire<T> t)
        {
            if (t is Wire<T> tt) return tt;
            return new Wire<T>((n, e) => t.Subscribe(n, e));
        }

        public static WireGetter<T> GetGetter<T>(this IWire<T> t)
        {
            return new WireGetter<T>(t);
        }

        //sketchy reusable thing
        public class WireGetter<T> : IDisposable
        {

            private IDisposable d;
            private T? v;
            private TaskCompletionSource<T> conts = new();

            public WireGetter(IWire<T> w)
            {
                d = w.Subscribe(complete);
            }
            private void complete(T vv)
            {
                Interlocked.Exchange(ref conts, new()).SetResult(vv);

            }

            public Task<T> Get()
            {
                return conts.Task;
            }

            public bool IsCompleted { get; private set; }

            public T GetResult() => v!;

            public void Dispose()
            {
                if (IsCompleted) return;
                conts = null;
                IsCompleted = true;
                d.Dispose();
            }

            ~WireGetter()
            {
                Dispose();
            }
        }
        //TODO implement custom INotify
        public static async Task<T> Get<T>(this IWire<T> t, TimeSpan? timeout = null)
        {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();

            using (t.Subscribe(o =>
             {
                 lock (taskCompletionSource)
                 {
                     if (taskCompletionSource.Task.IsCompleted)
                     {
                         System.Diagnostics.Debug.WriteLine("bad");
                         return;
                     }
                     taskCompletionSource.SetResult(o);
                 }
             },
             onErr: e =>
             {
                 lock (taskCompletionSource)
                 {
                     taskCompletionSource.SetException(e);
                 }
             }))
            {
                if (timeout is TimeSpan tt)
                    _ = Task.Delay(tt).ContinueWith(_ =>
                    {
                        if (!taskCompletionSource.Task.IsCompleted)
                            taskCompletionSource.SetException(new TimeoutException());
                    });
                return await taskCompletionSource.Task.ConfigureAwait(false);
            }
        }

        public static async Task WaitTrue(this ILiveWire<bool> t, TimeSpan? timeout = null)
        {
            if (t.Value) return;
            while (!await t.Get(timeout).ConfigureAwait(false)) ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="In"></typeparam>
        /// <typeparam name="Out"></typeparam>
        /// <param name="w"></param>
        /// <param name="f"></param>
        /// <param name="protect">If true, ignores calls to next when not subscribed</param>
        /// <returns></returns>
        public static IWire<Out> Link<In, Out>(this IWire<In> w, Action<In, Action<Out>> f, bool protect = false)
        {
            return new Wire<Out>((onNext, eH) =>
            {
                bool disposed = false;
                void next(Out v)
                {
                    if (protect)
                        if (disposed) return;
                    onNext(v);
                }
                var dis = w.Subscribe(value => f(value, next), eH);
                return DisposableUtil.From(() =>
                {
                    disposed = true; dis.Dispose();
                });
            });
        }



        public static ILiveWire<Out> Select<In, Out>(this ILiveWire<In> w, Func<In, Out> f)
        {
            return new LiveWire<Out>(() => f(w.Value), (onChange, eh) =>
                w.Subscribe(_ => onChange(), eh));
        }

        public static IWire<Out?> Select2<In, Out>(this IWire<In?> w, Func<In, Out> f)
     where In : struct
    where Out : struct

        {
            return w.Select<In?, Out?>(x => x is null ? null : f(x.Expect()));
        }

        public static ILiveWire<Out?> Select2<In, Out>(this ILiveWire<In?> w, Func<In, Out> f)
             where In : struct
            where Out : struct

        {
            Out? process()
            {
                if (w.Value is In i)
                    return f(i);
                return default;
            }
            return new LiveWire<Out?>(process, (onChange, eh) =>
                w.Subscribe(_ => onChange(), eh));
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
            return new LiveWire<Out?>(process, (onChange, eh) =>
                w.Subscribe(_ => onChange(), eh));
        }
        public static ILiveWire<Out?> Select3<In, Out>(this ILiveWire<In?> w, Func<In, Out> f)
           where In : class
          where Out : class

        {
            Out? process()
            {
                if (w.Value != null)
                    return f(w.Value);
                return default;
            }
            return new LiveWire<Out?>(process, (onChange, eh) =>
                w.Subscribe(_ => onChange(), eh));
        }
        public static ILiveWire<Out?> Select2<In, Out>(this ILiveWire<In?> w, Func<In, Out> f)
            where In : class
           where Out : struct

        {
            Out? process()
            {
                if (w.Value != null)
                    return f(w.Value);
                return default;
            }
            return new LiveWire<Out?>(process, (onChange, eh) =>
                w.Subscribe(_ => onChange(), eh));
        }

        public static IWire<double> Throughput<T>(this IWire<T> w, long sampleLen = 2000)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Queue<long> time = new();
            long recent;
            return w.Link((Action<T, Action<double>>)((_, next) =>
            {
                recent = sw.ElapsedMilliseconds;
                time.Enqueue(recent);
                while (time.Count > 0 && time.Peek() < recent - sampleLen)
                    time.Dequeue();
                if (time.Count > 1)
                {
                    next(time.Count * 1000.0 / sampleLen);
                }
            }));
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
        public static IWire<T> Catch<T>(this IWire<T> w, Action<Exception> f, bool bubble = true)
        {
            return new Wire<T>((n, eh) => w.Subscribe(n, e =>
            {
                f(e);
                if (bubble) eh(e);
            }));
        }
        public static ILiveWire<T> Do<T>(this ILiveWire<T> w, Action<T> f)
        {
            return w.Select(x => { f(x); return x; });
        }
        public static ILiveWire<T> Debug<T>(this ILiveWire<T> w, string tag)
        {
            void print(string s)
            {
                Console.WriteLine($"DBG: {s}");
            }
            return new LiveWire<T>(() =>
            {
                var res = w.Value;
                print($"{tag}.getVal() = {res}");
                return res;
            }, (onChange, eH) =>
            {
                print($"{tag}.enable = true");

                var d = w.Subscribe(x =>
                {
                    print($"{tag}.onChange({x})");
                    onChange();
                }, e =>
                {

                    print($"{tag}.onErr({e})");
                    eH(e);
                });

                return DisposableUtil.From(() =>
                {
                    print($"{tag}.enable = false");
                    d.Dispose();
                });
            });
        }
        public static IWire<NoneT> Nonify<T>(this IWire<T> w)
        {
            return w.Select(_ => NoneT.V);
        }
        public static IWire<T> Merge<T>(params IWire<T>[] w)
        {
            return new Wire<T>((onNext, eH) =>
            {
                var lst = new List<IDisposable>(w.Length);
                foreach (var wire in w)
                    lst.Add(wire.Subscribe(onNext, eH));
                return DisposableUtil.From(() =>
                {
                    foreach (var d in lst)
                        d.Dispose();
                });
            });
        }
        public static IWire<T> Debug<T>(this IWire<T> w, string tag)
        {
            void print(string s)
            {
                System.Diagnostics.Debug.WriteLine($"DBG: {s}");
            }
            return new Wire<T>((onChange, eH) =>
            {
                print($"{tag}.enable = true");

                var d = w.Subscribe(x =>
                {
                    print($"{tag}.onNext({x})");
                    onChange(x);
                }, e =>
                {

                    print($"{tag}.onErr({e})");
                    eH(e);
                });

                return DisposableUtil.From(() =>
                {
                    print($"{tag}.enable = false");
                    d.Dispose();
                });
            });
        }
        public static IWire<Out> Select<In, Out>(this IWire<In> w, Func<In, Out> f)
        {
            return w.Link<In, Out>((value, next) =>
                        next(f(value)));
        }
        public class ProcessAsyncOptions
        {
            /*  public enum ConcurrentMode
              {
                  /// <summary>
                  /// All async requests are run, with no extra stuff happening
                  /// </summary>
                  RunAll,

                  /// <summary>
                  /// A limited number of async
                  /// </summary>
                  Buffered
              }
              public ConcurrentMode Mode = ConcurrentMode.RunAll;*/

            /* public enum OverflowMode
             {
                 /// <summary>
                 /// Simply ignore new packets
                 /// </summary>
                 Drop,
             }

             /// <summary>
             /// what happens when tasks exceed concurrent limit
             /// </summary>
             public OverflowMode Overflow=OverflowMode.Drop;*/

            /// <summary>
            /// The amount of time to wait before dropping a packet
            /// in the case of exceeding MaxConcurrency
            /// </summary>
            public TimeSpan WaitSpot = TimeSpan.Zero;

            /// <summary>
            /// Max number of tasks allowed to run at same time
            /// </summary>
            public int? MaxConcurrency = 1;

            public bool IgnoreLateResults = true;
        }
        public static ProcessAsyncOptions DefaultAsyncOptions = new ProcessAsyncOptions();


        public static IWire<Out> LinkAsync<In, Out>(this IWire<In> w,
            Func<In, Action<Out>, Task> f,
            ProcessAsyncOptions? opt = null)
        {
            var opt2 = opt ?? DefaultAsyncOptions;
            if (opt2.MaxConcurrency is int mc)
            {
                //System.Diagnostics.Debug.Assert(mc == 1, "MaxConcurrency 1 only supported");

                var limiter = new SemaphoreSlim(mc);
                return w.Link<In, Out>(async (value, next) =>
                {
                    if (await limiter.WaitAsync(opt2.WaitSpot).ConfigureAwait(false))
                    {
                        try
                        {
                            await f(value, next).ConfigureAwait(false);

                        }
                        finally
                        {
                            limiter.Release();
                        }
                    }

                }, opt2.IgnoreLateResults);


            }
            else throw new NotSupportedException();
        }

        public static IWire<Out> ProcessAsync<In, Out>(this IWire<In> w, Func<In, Task<Out>> f,
             ProcessAsyncOptions? opt = null)
        {
            var opt2 = opt ?? DefaultAsyncOptions;
            if (opt2.MaxConcurrency is int mc)
            {
                //System.Diagnostics.Debug.Assert(mc == 1, "MaxConcurrency 1 only supported");

                var limiter = new SemaphoreSlim(mc);
                return w.Link<In, Out>(async (value, next) =>
                {
                    if (await limiter.WaitAsync(opt2.WaitSpot).ConfigureAwait(false))
                    {
                        try
                        {
                            next(await f(value).ConfigureAwait(false));

                        }
                        finally
                        {
                            limiter.Release();
                        }
                    }

                }, opt2.IgnoreLateResults);


            }
            else
                //TODO swallows stuff
                return w.Link<In, Out>(async (value, next) =>
                    next(await f(value).ConfigureAwait(false))
                , opt2.IgnoreLateResults);
        }
        public static IWire<Out> ProcessAsync<In, Out>(this IWire<In> w, Func<In, Out> f,
            ProcessAsyncOptions? opt = null)
        {
            //TODO swallows stuff
            return ProcessAsync(w, x => Task.Run(() => f(x)), opt);
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
            where In : struct
        {
            return w.Select<In, In?>((value) => f(value) ? value : default);
        }
        public static IWire<In> ToWire<In>(this IObservable<In> w)
        {
            return new Wire<In>((onNext, eH) => w.Subscribe(onNext, eH));
        }
    }
}

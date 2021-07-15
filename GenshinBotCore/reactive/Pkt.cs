using genshinbot.reactive.wire;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.reactive
{
    /// <summary>
    /// Utility class to wrap data passed through reactive streams.
    /// Provides features such as:
    ///  - Timestamp
    ///  - Lock/refcount?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pkt<T>:IComparable<Pkt<T>>
    {
        static DateTime ProgramStart=DateTime.Now;
        public T Value { get; init; }

        public DateTime CaptureTime { get; init; }

        public IObserver<(Pkt<T> dst, bool doLock)> Locker { get;  init; }

        public IDisposable Lock()
        {
            Debug.Assert(Locker != null);
            Locker.OnNext((this, true));
            return Disposable.Create(() => Locker.OnNext((this, false)));
        }

        public Pkt(T value) : this(value, DateTime.Now) { }

        public Pkt(T value, DateTime time)
        {
            CaptureTime = time;
            Value = value;
        }

        public Pkt<U> Select<U>(Func<T,U> transform)
        {
            return new Pkt<U>(transform(Value), CaptureTime);
        }

        public override string ToString()
        {
            var diff = CaptureTime - ProgramStart;
            return $"{diff} {Value}";
        }

        public int CompareTo(Pkt<T>? other)
        {
            return CaptureTime.CompareTo(other?.CaptureTime);
        }
    }


    public static class Pkt
    {
        public static async Task<T>Get<T>(this IWire<Pkt<T>> t)
        {
            return (await t.Get<Pkt<T>>().ConfigureAwait(false)).Value;
        }
        public static IWire<Pkt<T>> Where<T>(this IWire<Pkt<T>> t, Func<T,bool> v)
        {
            return t.Where(pkt => v(pkt.Value));
        }
        public static IDisposable Subscribe<T>(this IWire<Pkt<T>> t, Action<T> v)
        {
            return t.Subscribe(pkt => v(pkt.Value));
        }
        public static void Emit<T>(this WireSource<Pkt<T>> t, T v)
        {
            t.Emit(new Pkt<T>(v));
        }
        public static IWire<Pkt<bool>> AllLatest(this IWire<Pkt<bool>>[] t,
  Wire.CombineAsyncOptions? opt = null)
        {
            return CombineLatest(t, v => v.All(x => x), opt);
        }
        //TODO debounce based on packet time
        public static IWire<Pkt<T>> CombineLatest<T, In1>(this IWire<Pkt<In1>>[] t,
          Func<In1[], T> f, Wire.CombineAsyncOptions? opt = null)
        {
            int count = 0;
            bool[] bad = new bool[t.Length];
            In1[] last = new In1[t.Length];
            var thing = Wire.Merge(
                t.Select((wire, index) =>
                    wire.Do(val => {
                        if (!bad[index] ) Interlocked.Increment( ref count);
                        last[index] = val.Value;
                        bad[index] = true;
                       //Debug.WriteLine($"last={string.Join(',', last)}");
                    })
                )
            );

            object? lck = null;
            var opt2 = opt ?? Wire.DefaultCombineOptions;
            if (opt2.Lock) lck = new object();

            if (opt2.Debounce is int dd)
                thing = thing.Debounce(dd);

            return thing
                //.Do(_=>Debug.WriteLine($"badcount={Thread.VolatileRead(ref count)}"))
                .Where(_=>Thread.VolatileRead(ref count)==t.Length)
                .Select(_ =>
                {
                    if (lck is null) return f(last!);
                    lock (lck) return f(last);
                });
        }

        /// <summary>
        /// Performs transformation on stream of packets, keeping the CaptureTime of the packet the same
        /// </summary>
        /// <typeparam name="In"></typeparam>
        /// <typeparam name="Out"></typeparam>
        /// <param name="observable"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static IWire<Pkt<Out>> Select<In, Out>(
            this IWire<Pkt<In>> observable,
            Func<In, Out> fn)
        {
            return observable.Select((Pkt<In> x) => x.Select(fn));
        }
        public static ILiveWire<Pkt<Out>> Select<In, Out>(this ILiveWire<Pkt<In>> observable, Func<In, Out> fn)
        {
            return observable.Select((Pkt<In> x) => x.Select(fn));
        }
        public static IWire<Pkt<In>> Do<In>(this IWire<Pkt<In>> observable, Action<In> fn)
        {
            return observable.Do((Pkt<In> x) => fn(x.Value));
        }

        /// <summary>
        /// Produces a new stream with packet information discarded
        /// </summary>
        /// <typeparam name="In"></typeparam>
        /// <typeparam name="Out"></typeparam>
        /// <param name="observable"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static IWire<Out> Depacket<In, Out>(this IWire<Pkt<In>> observable, Func<In, Out> fn)
        {
            return observable.Select((Pkt<In> x) => fn(x.Value));
        }

        /// <summary>
        /// Produces a new stream with packet information discarded
        /// </summary>
        public static IWire<In> Depacket<In>(this IWire<Pkt<In>> observable)
        {
            return observable.Select((Pkt<In> x) => x.Value);
        }
        public static ILiveWire<In> Depacket<In>(this ILiveWire<Pkt<In>> observable)
        {
            return observable.Select((Pkt<In> x) => x.Value);
        }

        /// <summary>
        /// Wraps stream in Pkt, at current time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <returns></returns>
        public static IWire<Pkt<T>> Packetize<T>(this IWire<T> observable)
        {
            return observable.Select(x => new Pkt<T>(x));
        }
        public static ILiveWire<Pkt<T>> Packetize<T>(this ILiveWire<T> observable)
        {
            return observable.Select(x => new Pkt<T>(x));
        }
    }
    public static class PktObservableExtensions
    {
        public static IObservable<Pkt<Out>> ProcessAsync<In, Out>(this IObservable<Pkt<In>> observable, Func<In, Out> fn)
        {
            return observable.ProcessAsync(x => x.Select(fn));
        }


        /// <summary>
        /// Performs transformation on stream of packets, keeping the CaptureTime of the packet the same
        /// </summary>
        /// <typeparam name="In"></typeparam>
        /// <typeparam name="Out"></typeparam>
        /// <param name="observable"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static IObservable<Pkt<Out>> Select<In, Out>(this IObservable<Pkt<In>> observable, Func<In, Out> fn)
        {
            return observable.Select((Pkt<In> x) => x.Select(fn));
        }


        /// <summary>
        /// Produces a new stream with packet information discarded
        /// </summary>
        /// <typeparam name="In"></typeparam>
        /// <typeparam name="Out"></typeparam>
        /// <param name="observable"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static IObservable<Out> Depacket<In, Out>(this IObservable<Pkt<In>> observable, Func<In, Out> fn)
        {
            return observable.Select((Pkt<In> x) => fn(x.Value));
        }

        /// <summary>
        /// Produces a new stream with packet information discarded
        /// </summary>
        public static IObservable<In> Depacket<In>(this IObservable<Pkt<In>> observable)
        {
            return observable.Select((Pkt<In> x) => x.Value);
        }

        /// <summary>
        /// Wraps stream in Pkt, at current time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <returns></returns>
        public static IObservable<Pkt<T>> Packetize<T>(this IObservable<T> observable)
        {
            return observable.Select(x=>new Pkt<T>(x));
        }
    }
}

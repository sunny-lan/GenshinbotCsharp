using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
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
    public class Pkt<T>
    {
        public T Value { get; init; }

        public DateTime CaptureTime { get; init; }

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
    }

    public static class PktObservableExtensions
    {
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
            return observable.Select((Pkt<In> x) => new Pkt<Out>(fn(x.Value), x.CaptureTime));
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
    }
}

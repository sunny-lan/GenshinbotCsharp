﻿using System;
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

        public volatile int RefCount = 0;

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

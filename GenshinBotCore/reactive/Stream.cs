using genshinbot.reactive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;

namespace genshinbot
{
    namespace reactive
    {

        public interface Subscription : IDisposable { }

        /// <summary>
        /// An observable that stores it's last known value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface IObservableValue<T> : IObservable<T>
        {

            /// <summary>
            /// Stores last known value of observable
            /// </summary>
            T Value { get; }


        }
        public static class ObservableValue
        {
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

        public class ObservableValue<T> : IObservableValue<T>
        {
            private IObservable<T> wrapped;


            public T Value { get; private set; }

            /// <summary>
            /// Whether to give current Value whenever an observer subscribes. default true
            /// </summary>
            public bool GiveInitVal = true;

            public ObservableValue(IObservable<T> wrapped, T init)
            {
                this.wrapped = wrapped.Do(v => Value = v);
                Value = init;
            }


            public IDisposable Subscribe(IObserver<T> observer)
            {
                observer.OnNext(Value);
                return wrapped.Subscribe(observer);
            }

        }
    }


}

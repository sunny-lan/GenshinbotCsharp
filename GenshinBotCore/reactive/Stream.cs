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

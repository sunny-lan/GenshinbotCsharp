using System;
using System.Collections.Generic;
using System.Threading;

namespace genshinbot.util
{
    public class DisposableUtil
    {
        class Impl : IDisposable
        {
            public Action dispose { get; init; }
            private int disposed = 0;
            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1)==0) dispose();
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

        public static IDisposable Empty = From(() => { });

        /*public static IDisposable Merge(IEnumerable< IDisposable> dispose)
        {
            return new Impl
            {
                dispose = () =>
                {
                    foreach (var disposable in dispose)
                        disposable.Dispose();
                }
            };
        }*/
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
}

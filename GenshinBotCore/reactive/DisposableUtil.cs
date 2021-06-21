using System;

namespace genshinbot.util
{
    public class DisposableUtil
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

        public static IDisposable Empty = From(() => { });

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

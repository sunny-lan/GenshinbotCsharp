using genshinbot.reactive.wire;
using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.reactive
{
    public class Poller
    {
        public Wire<NoneT> wire { get; }
        public Poller(Func<Task> a, int? delay = null)
        {
            var lck = new SemaphoreSlim(1);
            wire= new Wire<NoneT>(_ => Poller.InfiniteLoop(async()=> {
                await lck.WaitAsync().ConfigureAwait(false);
                try
                {
                    await a();
                }
                finally
                {
                    lck.Release();
                }

            }, delay));
        }
        public Poller(Action a, int? delay = null)
        {
            wire = new Wire<NoneT>(_ => Poller.InfiniteLoop(a, delay));
        }
        public static IDisposable InfiniteLoop(Action a, int? delay = null)
        {
            return InfiniteLoop(async () => a(), delay);
        }
        public static IDisposable InfiniteLoop(Func<Task> a, int? delay=null)
        {
            var ts = new CancellationTokenSource();
            Task.Run(async() => {
               // Console.WriteLine("begin loop");
                while (!ts.Token.IsCancellationRequested)
                {
                    //   Console.WriteLine(" begin poll");
                    await a().ConfigureAwait(false);
                    if (delay is int dd)
                        await Task.Delay(dd, ts.Token).ConfigureAwait(false);
                }
               // Console.WriteLine("end loop");
            }, ts.Token);
            return DisposableUtil.From(() => {
                Console.WriteLine("request cancellation");
                ts.Cancel();

            });
        }

        #region tests
        /// <summary>
        /// No interval, task takes 1000ms
        /// </summary>
        public static void Test1()
        {
            int i = 0;
            var stream = new Poller<int>(() =>
            {
                Thread.Sleep(1000);
                return i++;
            });


            using (stream.Wire.Subscribe(x => Console.WriteLine(x)))
            {
                Thread.Sleep(10000);
            }
        }
        /// <summary>
        /// Task takes 0ms, interval is 500ms
        /// </summary>
        public static void Test2()
        {
            int i = 0;
            var stream = new Poller<int>(() =>
            {
                return i++;
            });
            stream.Interval = 500;

            using (stream.Wire.Subscribe(x => Console.WriteLine(x)))
            {
                Thread.Sleep(5000);
            }
        }
        /// <summary>
        /// Task takes 1000ms, no interval, max in flight=2
        /// </summary>
        public static void Test3()
        {
            int i = 0;
            var stream = new Poller<int>(() =>
            {
                Thread.Sleep(1000);
                return i++;
            });
            stream.MaxInFlight = 2;

            using (stream.Wire.Subscribe(x => Console.WriteLine(x)))
            {
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// multiple tasks in flight where task takes longer than interval
        /// </summary>
        public static void Test4()
        {
            int i = 0;
            var stream = new Poller<int>(() =>
            {
                Thread.Sleep(1000);
                return i++;
            });
            stream.Interval = 100;
            stream.MaxInFlight = 3;
            using (stream.Wire.Subscribe(x => Console.WriteLine(x)))
            {
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// check if poll starts and stops properly
        /// </summary>
        public static void Test5()
        {
            int i = 0;
            var stream = new Poller<int>(() =>
            {
                Console.WriteLine("poll");
                return i++;
            });
            stream.Interval = 100;
            stream.MaxInFlight = 3;

            Console.WriteLine("poll should begin after");
            using (stream.Wire.Subscribe(x => Console.WriteLine(x)))
            {
                Thread.Sleep(5000);
            }
            Console.WriteLine("no poll should be");
            Thread.Sleep(2000);

            Console.WriteLine("poll should begin after");
            using (stream.Wire.Subscribe(x => Console.WriteLine(x)))
            {
                Thread.Sleep(5000);
            }
            Console.WriteLine("no poll should be");
            Thread.Sleep(5000);
        }
        public static void Test()
        {
            Test5();
        }
        #endregion
    }
    /// <summary>
    /// Polls the value of a function and outputs it into a stream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Poller<T> 
    {
        public ILiveWire<T> Wire => obsSubj;
        private LiveWire<T> obsSubj;
        Func<T> poll;
      
        public Poller(Func<T> poll)
        {
            this.poll = poll;
            //  obsSubj = new LiveWire<T>(poll, Poller.InfiniteLoop);
            throw new NotImplementedException();
        }


        /// <summary>
        /// Polling interval, in milliseconds.
        /// If 0, the maximum speed possible is used
        /// </summary>
        public int Interval
        {
            get => 0; set => throw new NotImplementedException();
        }

        public int MaxInFlight
        {
            get => 1; set
            {
                if (value != 1) throw new NotImplementedException();
            }
        }

    }

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.stream
{
    /// <summary>
    /// Polls the value of a function and outputs it into a stream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Poller<T> : Stream<T>
    {
        Func<T> poll;
        public Poller(Func<T> poll) : base(poll())
        {
            this.poll = poll;
        }
        public Poller(Func<T> poll, T init) : base(init)
        {
            this.poll = poll;
        }

        public override Action<bool> EnableChanged => enableChanged;
        private bool running = false;
        private Task poller;

        /// <summary>
        /// Polling interval, in milliseconds.
        /// If 0, the maximum speed possible is used
        /// </summary>
        public int Interval = 0;

        public int MaxInFlight
        {
            get => maxInFlight; set
            {
                Debug.Assert(value >= maxInFlight, "MaxInFlight cannot be decreased!");
                semaphore.Release(value - maxInFlight);
                maxInFlight = value;
            }
        }

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private int maxInFlight = 1;

        private void enableChanged(bool enabled)
        {
            if (enabled)
            {
                running = true;
                poller = Task.Run(pollLoop);
            }
            else
            {
                running = false;
                //TODO this could take a while depending on MaxInFlight
                poller?.Wait();
            }
        }

        private async Task pollLoop()
        {
            await semaphore.WaitAsync();
            while (running)
            {
                _ = Task.Run(() =>
                {
                    Update(poll());
                    semaphore.Release();
                });
                //Fire a new poll task when both the delay and the semaphore are done
                await Task.WhenAll(Task.Delay(Interval), semaphore.WaitAsync());
            }
        }

    }

    public static class Poller
    {
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


            using (var listener = stream.Listen(x => Console.WriteLine(x)))
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

            using (var listener = stream.Listen(x => Console.WriteLine(x)))
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

            using (var listener = stream.Listen(x => Console.WriteLine(x)))
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
                Console.WriteLine("poll");
                Thread.Sleep(1000);
                return i++;
            });
            stream.Interval = 100;
            stream.MaxInFlight = 3;
            Console.WriteLine("poll should begin after");
            using (var listener = stream.Listen(x => Console.WriteLine(x)))
            {
                Thread.Sleep(5000);
            }
            Console.WriteLine("no poll should be");
            Thread.Sleep(2000);
        }
        public static void Test()
        {
            Test4();
        }
    }
}

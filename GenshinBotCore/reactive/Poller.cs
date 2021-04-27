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
    /// <summary>
    /// Polls the value of a function and outputs it into a stream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Poller<T> : IObservable<T>
    {
        private Subject<T> subject;
        private IObservable<T> obsSubj;
        Func<T> poll;
        public Poller(Func<T> poll)
        {
            this.poll = poll;
            subject = new Subject<T>();
            //dummy observable to enable/disable
            var obs = Observable.FromEvent<T>(h => enableChanged(true), h => enableChanged(false));
            obsSubj = Observable.Merge(obs, subject);

        }

        private volatile bool running = false;
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
                if (value == maxInFlight) return;
                Debug.Assert(value > maxInFlight, "MaxInFlight cannot be decreased!");
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
                Debug.Assert(poller == null);

                running = true;
                poller = Task.Run(pollLoop);
            }
            else
            {
                Debug.Assert(poller != null);

                running = false;
                //TODO this could take a while depending on MaxInFlight
                poller.Wait();
                poller = null;
            }
        }

        private async Task pollLoop()
        {
            Task delay = Task.CompletedTask;
            while (running)
            { 
                //Fire a new poll task when both the delay and the semaphore are done
                await Task.WhenAll(delay, semaphore.WaitAsync());
                //the delay for the next task starts as soon as the task launches
                delay = Task.Delay(Interval);
                _ = Task.Run(() =>
                  {
                      try
                      {
                          if (!running) return;
                          var v = poll();
                          if (!running) return;
                          subject.OnNext(v);
                      }
                      catch (Exception e)
                      {
                          subject.OnError(e);
                      }
                      finally
                      {
                          semaphore.Release();
                      }
                  });
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return obsSubj.Subscribe(observer);
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


            using (stream.Subscribe(x => Console.WriteLine(x)))
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

            using (stream.Subscribe(x => Console.WriteLine(x)))
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

            using (stream.Subscribe(x => Console.WriteLine(x)))
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
            using (stream.Subscribe(x => Console.WriteLine(x)))
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
            using (stream.Subscribe(x => Console.WriteLine(x)))
            {
                Thread.Sleep(5000);
            }
            Console.WriteLine("no poll should be");
            Thread.Sleep(2000);

            Console.WriteLine("poll should begin after");
            using (stream.Subscribe(x => Console.WriteLine(x)))
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
    }
}

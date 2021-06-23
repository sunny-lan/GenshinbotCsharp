using genshinbot.diag;
using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.reactive.wire
{
    public class Wire<T> : IWire<T>, IObservable<T>
    {
        WireDebug.Info? dbg = WireDebug.Instance.GetDebug();

        volatile List<Action<T>> subscribers = new List<Action<T>>();
        Func<Action<T>, IDisposable> Enable;
      //  SemaphoreSlim enabled_lock = new SemaphoreSlim(1);
        SemaphoreSlim S_lock = new SemaphoreSlim(1);
        //TODO performance optimization for single subscriber case
        public Wire(Func<Action<T>, IDisposable> enable)
        {
            Enable = enable;
        }

        volatile IDisposable? enabled;
        volatile bool _enabled;
       // int pendingStatus = 0;
       // SemaphoreSlim pendingLock=new SemaphoreSlim(1);
        protected virtual void OnEnable(bool e)
        {



            if (e)
            {
               // enabled_lock.Wait();
                Debug.Assert(!_enabled);
                _enabled = true;
                //Action<T> k = OnNext;
               // Console.WriteLine($"{iid=k.GetHashCode()} enabled");
                enabled = Enable.Invoke(OnNext);
            //    enabled_lock.Release();
            }
            else
            {
             /*   pendingLock.Wait();
                Debug.Assert(Interlocked.Exchange(ref pendingStatus, 1) == 0);

                pendingLock.Release();*/
             //   enabled_lock.Wait();
                Debug.Assert(_enabled);
                Debug.Assert(enabled is not null);
                enabled.Dispose();
                enabled = null;
                _enabled = false;
               // Console.WriteLine($"{iid} disabled");
                //Debug.Assert(Interlocked.Exchange(ref pendingStatus, 0) == 1);


            //    enabled_lock.Release();
            }
        }
        //Task? nLock;
       // object n_lock = new object();
        protected virtual void OnNext(T value)
        {
         //   enabled_lock.Wait();
            /*try
            {
                pendingLock.Wait();
                Debug.Assert(Thread.VolatileRead(ref pendingStatus) == 0);
                Console.WriteLine($"{iid} onNext lock enter");
                Console.WriteLine($"{iid} onNext lock aquire");
            }
            finally
            {
                pendingLock.Release();
            }*/
            Debug.Assert(_enabled);
            List<Action<T>> tmp;

            //  Console.WriteLine($"{iid} onNext slock enter");
            //    S_lock.Wait();
            //   TaskCompletionSource sc = new TaskCompletionSource();
       //     lock (n_lock)
            {
                lock (S_lock)
                {
                    tmp = subscribers; 
              //      Console.WriteLine($"{iid} onNext slock {tmp.Count}");

                    //       nLock = sc.Task;
                }

                // Console.WriteLine($"{iid} onNext slock exit");
                //   S_lock.Release();
                //

                List<Exception> e = null;
                foreach (var s in tmp)
                {
                    try
                    {
                //        Console.WriteLine($"{iid} call subscriber {s.GetHashCode()}");
                        s(value);
                    }
                    catch (Exception ee)
                    {
                        if (e == null) e = new List<Exception>();
                        e.Add(ee);
                    }
                }
            if (e != null)
                throw new AggregateException(e);
            }
          //  Console.WriteLine($"{iid} onNext slock exit");
            //    sc.SetResult();
            //      nLock = null;

            // Console.WriteLine($"{iid} onNext lock exit");
            //     enabled_lock.Release();

        }



        public IDisposable Subscribe(Action<T> onValue)
        {
            bool disposed=false;
            object lck = new object();
            void wrapper(T v)
            {
                lock (lck)
                    if (!disposed)
                        onValue(v);
                    else
                        Console.WriteLine("BAD");
            }

            int count;
            //  Console.WriteLine($"{iid} subscribe lock enter");
            //  S_lock.Wait();
            //  Console.WriteLine($"{iid} subscribe lock acq");
            lock (S_lock)
            {
                subscribers = new List<Action<T>>(subscribers);
                subscribers.Add(wrapper);
                count = subscribers.Count;
            }
          //  Console.WriteLine($"{iid} subscribe lock exit");
         //   S_lock.Release();
            if (count == 1)
            {
                OnEnable(true);
            }
            return DisposableUtil.From(() =>
            {
                lock(lck)
                disposed = true;
                //Console.WriteLine($"{iid} remove subscriber {onValue.GetHashCode()} begin");

               // if (iid == 1499603965 && onValue.GetHashCode() == 1969696357)
                 //   Debug.Assert(false);
                    int c2;
                // S_lock.Wait();
                // Task? n_;
             //   lock (n_lock)
                {
                    lock (S_lock)
                    {
                        subscribers = new List<Action<T>>(subscribers);
                        subscribers.Remove(wrapper);
                        c2 = subscribers.Count;
                        //      n_ = nLock;
                    }
                    // S_lock.Release();
                    if (c2 == 0)
                    {
                        OnEnable(false);
                    }
                }
                //   n_?.Wait();
                //Console.WriteLine($"{iid} remove subscriber {onValue.GetHashCode()} end");

            });

        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return Subscribe(v => observer.OnNext(v));
        }

        void Dispose()
        {
            if (enabled != null)
                OnEnable(false);
        }

        ~Wire()
        {
            Dispose();
        }


    }
}

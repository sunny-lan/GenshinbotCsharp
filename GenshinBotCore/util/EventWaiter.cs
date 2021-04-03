using System;
using System.Threading;
using System.Threading.Tasks;

namespace GenshinbotCsharp.util
{
    /// <summary>
    /// Class to block a thread until a event is fired
    /// </summary>
    /// <typeparam name="T">The type of the eventarg</typeparam>
    public class EventWaiter<T> where T : struct
    {
        private object lck = new object();
        private T value;
        private bool has = false;

        /// <summary>
        /// blocks until a event is recieved or timeout elapses
        /// </summary>
        /// <param name="timeout">timeout, in millis. if -1, then waits forever</param>
        /// <returns></returns>

        public T WaitEvent(int timeout = -1)
        {
            lock (lck)
            {
                //wait for value to be set
                if (timeout == -1)
                {
                    Monitor.Wait(lck);
                }
                else
                {
                    bool v = Monitor.Wait(lck, timeout);
                    if (!v)
                        throw new System.TimeoutException("Waiting for event timed out");
                }
                return value;
            }
        }

        public T Once(int timeout = -1)
        {
            lock (lck)
            {
                if (has) return value;
            }
            return WaitEvent(timeout);
        }

        public void Reset()
        {
            lock (lck) has = false;
        }

        public void Signal(T evt)
        {
            lock (lck)
            {
                value = evt;
                has = true;
                Monitor.PulseAll(lck);
            }
        }

    }

    public static class EventWaiter
    {
        public static (Task<K>, Action<K>) Waiter<K>()
        {
            TaskCompletionSource<K> t = new TaskCompletionSource<K>();
            return (
                t.Task,
                x => t.TrySetResult(x)
            );
        }
        public static Task<K> WaitEvent<K>(Action<Action<K>> subscribe, Action<Action<K>> unsubscribe)
        {
            var (t, a) = Waiter<K>();
            Action<K> x = null;
            x = y =>
              {
                  a(y);
                  unsubscribe(x);
              };
            subscribe(x);
            return t;
        }
    }
}

using System.Threading;

namespace GenshinbotCsharp.hooks
{
    /// <summary>
    /// Class to block a thread until a event is fired
    /// </summary>
    /// <typeparam name="T">The type of the eventarg</typeparam>
    class EventWaiter<T> where T:struct
    {
        private object lck = new object();
        private T value;
        
        /// <summary>
        /// blocks until a event is recieved or timeout elapses
        /// </summary>
        /// <param name="timeout">timeout, in millis. if -1, then waits forever</param>
        /// <returns></returns>

        public T WaitEvent(int timeout=-1)
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

        public void Signal(T evt)
        {
            lock (lck)
            {
                value = evt;
                Monitor.PulseAll(lck);
            }
        }
    }
}

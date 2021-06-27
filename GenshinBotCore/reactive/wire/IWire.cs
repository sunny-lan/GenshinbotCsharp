using genshinbot.algorithm;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace genshinbot.reactive.wire
{
    public interface IWire<out T>
    {
        /// <summary>
        /// must be thread safe to call from multiple threads
        /// cannot be called with the same callback twice (aka before the previous disposed)
        /// </summary>
        /// <param name="onValue"></param>
        /// <returns></returns>
        IDisposable Subscribe(Action<T> onValue);
    }
}

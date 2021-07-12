using System;

namespace genshinbot.reactive.wire
{
    /// <summary>
    /// A wire, but always has a well defined value
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILiveWire<out T> : ILiveWire_WARNING<T> { }

    /// <summary>
    /// A subset of LiveWire which MUST be subscribed to function
    /// Temporary workaround
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILiveWire_WARNING<out T> : IWire<T>
    {
        public T Value { get; }


       
    }
}

using System;
using System.Linq;
using System.Text;

namespace genshinbot.reactive.wire
{
    public interface IWire<out T>
    {
        IDisposable Subscribe(Action<T> onValue);
    }
}

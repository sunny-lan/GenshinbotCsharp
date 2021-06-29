using genshinbot.automation.input;
using genshinbot.data.events;
using genshinbot.reactive.wire;
using System;

namespace genshinbot.automation.hooking
{

    public interface IMouseCapture
    {
       

        public IWire<MouseEvent> MouseEvents { get; }


    }
}

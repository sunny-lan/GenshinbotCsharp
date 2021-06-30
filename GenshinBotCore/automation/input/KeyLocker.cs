using genshinbot.automation.hooking;
using genshinbot.data.events;
using genshinbot.reactive.wire;
using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation.input
{
    public class KeyLocker
    {
        public IWire<KeyEvent> Event { get; }
        public KeyLocker(
            IWire<KeyEvent> evt,
            ILiveWire<bool> focused
        ) 
        {

            IDictionary<Keys, bool> last = new Dictionary<Keys,bool>();
            Event = Wire.Merge(
                evt.Link<KeyEvent,KeyEvent>((evt,next)=> {
                    last[evt.Key] = evt.Down;
                    if (focused.Value)
                        next(evt);
                }),

                focused.Edge(rising: true).Link<NoneT, KeyEvent>((_, next) =>
                {
                    foreach (var kvp in last)
                    {
                        if (kvp.Value)
                            next(new KeyEvent
                            {
                                Down = kvp.Value,
                                Key = kvp.Key
                            });
                    }
                })
            );
        }

    }
}

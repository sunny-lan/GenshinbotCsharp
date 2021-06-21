using genshinbot.automation.input;
using genshinbot.reactive.wire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace genshinbot.automation.hooking
{
    public interface IKeyCapture
    {
        public record KeyEvent
        {
            public Keys Key { get; init; }
            public bool Down { get; init; }
        }

        public IWire<KeyEvent> KeyEvents { get; }
        public ILiveWire<IReadOnlyDictionary<Keys, bool>> KbdState => KeyEvents.KbdState();
    }

    public static class KeyExt
    {
        public static ILiveWire<IReadOnlyDictionary<Keys,bool>> KbdState(this IWire<IKeyCapture.KeyEvent> o)
        {
            Dictionary<Keys, bool> d = new Dictionary<Keys, bool>();
            return o.Do(x => d[x.Key] = x.Down)
                .ToLive(() => d);
        }
        public static ILiveWire<bool> KeyCombo(this ILiveWire<IReadOnlyDictionary<Keys, bool>> o, params Keys[] combo)
        {
            return o.Select(
                st => combo.All(k=>st.GetValueOrDefault(k,false))
                && st.Count(x=>x.Value)==combo.Length);
        }

    }
}

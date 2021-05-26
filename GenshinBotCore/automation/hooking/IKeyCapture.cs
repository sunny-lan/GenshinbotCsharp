using genshinbot.automation.input;
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

        public IObservable<KeyEvent> KeyEvents { get; }
        public IObservable<IReadOnlyDictionary<Keys, bool>> KbdState => KeyEvents.KbdState();
    }

    public static class KeyExt
    {
        public static IObservable<IReadOnlyDictionary<Keys,bool>> KbdState(this IObservable<IKeyCapture.KeyEvent> o)
        {
            Dictionary<Keys, bool> d = new Dictionary<Keys, bool>();
            return o.Select(x =>
            {
                d[x.Key] = x.Down;
                return d;
            });
        }
        public static IObservable<IReadOnlyDictionary<Keys, bool>> KeyCombo(this IObservable<IReadOnlyDictionary<Keys, bool>> o, Keys[] combo)
        {
            return o.Where(st => combo.All(k=>st.GetValueOrDefault(k,false)) && st.Count(x=>x.Value)==combo.Length);
        }
    }
}

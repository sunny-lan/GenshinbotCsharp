﻿using genshinbot.automation.input;
using genshinbot.data.events;
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

        public IWire<KeyEvent> KeyEvents { get; }
        public ILiveWire<IReadOnlyDictionary<Keys, bool>> KbdState => KeyEvents.KbdState();
    }

    public static class KeyExt
    {
        public static ILiveWire<IReadOnlyDictionary<Keys,bool>> KbdState(this IWire<KeyEvent> o)
        {
            Dictionary<Keys, bool> d = new Dictionary<Keys, bool>();
            return o
                .Where(x=>!d.ContainsKey(x.Key) || d[x.Key]!=x.Down)
                .Do(x => d[x.Key] = x.Down)
                .ToLive(() => d);
        }
        public static ILiveWire<bool> KeyCombo(this ILiveWire<IReadOnlyDictionary<Keys, bool>> o, params Keys[] combo)
        {
            return o.Select(st => {
                return combo.All(k => st.GetValueOrDefault(k, false))
                   && st.Count(x => x.Value) == combo.Length;
            }).DistinctUntilChanged();
        }

    }
}

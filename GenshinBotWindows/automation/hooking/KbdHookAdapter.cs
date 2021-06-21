using genshinbot.automation.input;
using genshinbot.hooks;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Vanara.PInvoke;

namespace genshinbot.automation.hooking
{
    public class KbdHookAdapter:IKeyCapture
    {
        public IWire<IKeyCapture.KeyEvent> KeyEvents { get; private init; }
        public ILiveWire<IReadOnlyDictionary<Keys, bool>> KbdState => kbdState;


        public KbdHooker kbdHook;
        private ILiveWire<IReadOnlyDictionary<Keys, bool>> kbdState;

        public KbdHookAdapter(ILiveWire<bool> enabled)
        {
            kbdHook = new KbdHooker();

            KeyEvents = kbdHook.Wire
                .Relay(enabled)
                .Select(x => new IKeyCapture.KeyEvent
                {
                    Down = (User32.WindowMessage)x.wParam == User32.WindowMessage.WM_KEYDOWN,
                    Key = (input.Keys)x.lParam.vkCode,
                });
            kbdState = KeyEvents.KbdState();
            kbdHook.Start();
        }

        ~KbdHookAdapter()
        {
            kbdHook.Stop();
        }
    }
}

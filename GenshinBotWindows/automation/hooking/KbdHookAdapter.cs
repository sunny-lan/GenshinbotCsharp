using genshinbot.automation.input;
using genshinbot.hooks;
using genshinbot.reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Vanara.PInvoke;

namespace genshinbot.automation.hooking
{
    public class KbdHookAdapter:IKeyCapture
    {
        public IObservable<IKeyCapture.KeyEvent> KeyEvents { get; private init; }
        public IObservable<IReadOnlyDictionary<Keys, bool>> KbdState => kbdState;


        public KbdHooker kbdHook;
        private IObservable<IReadOnlyDictionary<Keys, bool>> kbdState;

        public KbdHookAdapter(IObservable<bool> enabled)
        {
            kbdHook = new KbdHooker();

            KeyEvents = kbdHook
                .Relay(enabled)
                .Select(x => new IKeyCapture.KeyEvent
                {
                    Down = (User32.WindowMessage)x.wParam == User32.WindowMessage.WM_KEYDOWN,
                    Key = (input.Keys)x.lParam.vkCode,
                });
            kbdState=KeyEvents.KbdState();
            kbdHook.Start();
        }

        ~KbdHookAdapter()
        {
            kbdHook.Stop();
        }
    }
}

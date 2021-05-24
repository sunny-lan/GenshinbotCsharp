﻿using genshinbot.automation.input;
using genshinbot.automation.screenshot;
using genshinbot.reactive;
using OpenCvSharp;
using System;
using System.Diagnostics;

namespace genshinbot.automation
{
    public class ProxyWAutomator : IWindowAutomator2
    {
        IObservableValue<bool> enabled;
        IWindowAutomator2 w;


        public IObservable<bool> Focused { get; private init; }

        public IObservable<Size> Size { get; private init; }

        public IKeySimulator2 Keys { get; private init; }

        public IMouseSimulator2 Mouse { get; private init; }

        public ScreenshotObservable Screen { get; private init; }

        public ProxyWAutomator(IObservableValue<bool> enabled, IWindowAutomator2 w)
        {
            this.enabled = enabled;
            this.w = w;
            Keys = new ProxyKey(enabled, w.Keys);
            Mouse = new ProxyMouse(enabled, w.Mouse);
            Screen = new ProxyScreen(enabled, w.Screen);

            Focused = w.Focused.Relay(enabled);
            Size = w.Size.Relay(enabled);

        }
        public void TryFocus()
        {
            Debug.Assert(enabled.Value);
            w.TryFocus();
        }
    }
}
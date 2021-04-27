using genshinbot.automation;
using genshinbot.automation.input;
using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot
{
    public interface BotInterface
    {
        /// <summary>
        /// Raw interface to window automiation
        /// </summary>
        IWindowAutomator2 W { get; }

        /// <summary>
        /// Proxied (rate limited/humanized) input
        /// </summary>
        IKeySimulator2 K => W.Keys;

        /// <summary>
        /// Proxied (rate limited/humanized) input
        /// </summary>
        IMouseSimulator2 M => W.Mouse;
    }
}

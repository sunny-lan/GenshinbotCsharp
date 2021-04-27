using genshinbot.automation;
using genshinbot.automation.input;
using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot
{
    /// <summary>
    /// Represents standard I/O interface to game
    /// </summary>
    public interface BotIO
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

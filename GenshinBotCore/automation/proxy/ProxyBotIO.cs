using genshinbot.automation.input;
using genshinbot.reactive;
using genshinbot.reactive.wire;

namespace genshinbot.automation
{
    public class ProxyBotIO : BotIO
    {
        ILiveWire<bool> enabled;

        public IWindowAutomator2 W { get; private init; }

        public IMouseSimulator2 M { get; private init; }
        public IKeySimulator2 K { get; private init; }

        public ProxyBotIO(ILiveWire<bool> enabled, BotIO io)
        {
            this.enabled = enabled;
            W = new ProxyWAutomator(enabled, io.W);
            M = new ProxyMouse(enabled, io.M);
            K = new ProxyKey(enabled, io.K);
        }


    }
}

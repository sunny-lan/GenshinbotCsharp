using genshinbot.automation.input;
using genshinbot.reactive;

namespace genshinbot.automation
{
    public class ProxyBotIO : BotIO
    {
        IObservableValue<bool> enabled;

        public IWindowAutomator2 W { get; private init; }

        public IMouseSimulator2 M { get; private init; }
        public IKeySimulator2 K { get; private init; }

        public ProxyBotIO(IObservableValue<bool> enabled, BotIO io)
        {
            this.enabled = enabled;
            W = new ProxyWAutomator(enabled, io.W);
            M = new ProxyMouse(enabled, io.M);
            K = new ProxyKey(enabled, io.K);
        }


    }
}

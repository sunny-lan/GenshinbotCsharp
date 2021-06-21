using genshinbot.automation.input;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using System.Diagnostics;
using System.Threading.Tasks;

namespace genshinbot.automation
{
    public class ProxyKey : IKeySimulator2
    {
        ILiveWire<bool> enabled;
        IKeySimulator2 kq;

        public ProxyKey(ILiveWire<bool> enabled, IKeySimulator2 kq)
        {
            this.enabled = enabled;
            this.kq = kq;
        }


        public Task Key(Keys k, bool down)
        {
            Debug.Assert(enabled.Value);
            return kq.Key(k, down);
        }
    }
}

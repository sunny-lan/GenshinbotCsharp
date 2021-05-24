using genshinbot.automation.input;
using genshinbot.reactive;
using System.Diagnostics;
using System.Threading.Tasks;

namespace genshinbot.automation
{
    public class ProxyKey : IKeySimulator2
    {
        IObservableValue<bool> enabled;
        IKeySimulator2 kq;

        public ProxyKey(IObservableValue<bool> enabled, IKeySimulator2 kq)
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

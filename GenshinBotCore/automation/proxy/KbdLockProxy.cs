using genshinbot.automation.input;
using genshinbot.data.events;
using genshinbot.reactive.wire;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation.proxy
{
    /// <summary>
    /// Locks the kbd state while window is unfocused
    /// </summary>
    public class KbdLockProxy : IKeySimulator2,IDisposable
    {
        private readonly IDisposable disp;
        private readonly IKeySimulator2 output;

        Dictionary<Keys, bool> kbdSt=new Dictionary<Keys, bool>();
        public int initialDelay { get; init; } = 100;

        public KbdLockProxy(IKeySimulator2 output, ILiveWire<bool > focused)
        {
            this.output = output;

            //repress all keys when focus is true again
            disp = focused.Edge(rising: true).Subscribe(async _ =>
            {
                Debug.WriteLine("Focus true. Repressing all keys!");
                await Task.Delay(initialDelay);
                await Task.WhenAll(kbdSt.Select(async k =>
                {
                    if (k.Value)
                    {
                        Debug.WriteLine($"   Repressing {k.Key}");
                        await output.Key(k.Key, k.Value);
                    }
                }));
            });
        }
        public void Dispose()
        {
            disp.Dispose();
        }

        public async Task Key(Keys k, bool down)
        {
            kbdSt[k] = down;
            await output.Key(k, down);
        }
    }
}

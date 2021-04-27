using genshinbot.automation;
using genshinbot.automation.windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.util
{
    class TestingRig : ITestingRig
    {
        class Bb : BotIO
        {
            public IWindowAutomator2 W { get; set; }
        }
        public BotIO Make()
        {
            return new Bb
            {
                W = new WindowAutomator2("Genshin Impact", "UnityWndClass")
            };
        }
    }
}

using genshinbot;
using genshinbot.automation;
using genshinbot.diag;
using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace GenshinBotTests.windows
{
    class TestUtil
    {
        public static ITestingRig DoInit()
        {
            DPIAware.Set(SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            //TaskExceptionCatcher.Do ();
            Kernel32.AllocConsole();
            return new TestingRig();
        }
    }
}

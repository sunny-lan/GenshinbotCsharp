using genshinbot.tools;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GenshinBotTests.windows
{
    public class AutofillToolTest
    {
        [Fact]
        public static async Task ConfigurePlayingScreen()
        {
            var bio = TestUtil.DoInit();
            await AutofillTool.ConfigurePlayingScreen(bio.Make());
        }
    }
}

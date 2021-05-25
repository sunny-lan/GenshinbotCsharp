using genshinbot.automation;
using System.Text;

namespace genshinbot.diag
{
    public class MockBotIO : BotIO
    {
        MockGenshinWindow g;

        public MockBotIO(MockGenshinWindow g)
        {
            this.g = g;
        }

        public IWindowAutomator2 W => g;
    }
    public class MockTestingRig : ITestingRig
    {
        MockGenshinWindow g;

        public MockTestingRig(MockGenshinWindow g)
        {
            this.g = g;
        }

        public BotIO Make()
        {
            return new MockBotIO(g);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

using genshinbot.tools;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Collections.Generic;

namespace genshinbot.tests
{
    [TestClass]
    public class AutofillToolTest
    {
        class TestObj
        {
            public class RD
            {
                public Point2d p2d { get; set; }

                public Rect poo { get; set; }

                public Mat derp { get; set; }
                public data.Snap derp1 { get; set; }

            }
            public Dictionary<Size, RD> Rd { get; set; } = new Dictionary<Size, RD>();
        }

        [TestMethod]
        public static async Task Test2()
        {
            var notepad = new automation.windows.WindowAutomator2("*Untitled - Notepad", null);
            var tool = new AutofillTool(notepad);

            await tool.Edit(new TestObj());
        }
    }

}

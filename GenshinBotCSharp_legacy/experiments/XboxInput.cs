using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenshinbotCsharp.experiments
{
    class XboxInput
    {
        public static void Run()
        {
            ViGEmClient client = new ViGEmClient();
            var ds4=client.CreateDualShock4Controller();
            ds4.Connect();
            Console.WriteLine("a");
            Console.ReadLine();
            Thread.Sleep(5000);
            ds4.SetButtonState(DualShock4Button.Circle, true);

            Thread.Sleep(100);

            ds4.SetButtonState(DualShock4Button.Circle, false);
            Console.WriteLine("b");
            Console.ReadLine();

        }
    }
}

using genshinbot.automation;
using genshinbot.automation.hooking;
using genshinbot.automation.input;
using genshinbot.data;
using genshinbot.reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
   public static class ScreencoordRecorder
    {
        public static async Task runAsync(IWindowAutomator2 w)
        {
            Keys[] take = { Keys.LControlKey, Keys.J };
            Keys[] stop = { Keys.LControlKey, Keys.K };
            Keys[] save = { Keys.LControlKey, Keys.S };
            var kbd = w.KeyCap.KbdState;
            while(true)
            {
                var k =await await Task.WhenAny(
                   kbd.KeyCombo(take).Select(_=>take).Get(),
                   kbd.KeyCombo(stop).Select(_=>stop).Get(),
                   kbd.KeyCombo(save).Select(_=>save).Get()
                );
                if (k == stop) break;
                if (k == take)
                {
                    var pos = await w.Mouse.MousePos();
                    var sz = await w.Size.Get();
                    Console.Write("name? ");
                    var name = Console.ReadLine();

                    Data.General.Root.Add(name, sz, pos, true);
                    //var line= $"public static Point2d P{name} = new Point2d({pos.X},{pos.Y});\n";
                    //Console.Write(line);
                    //res += line;
                }   else if (k == save)
                {
                    //TODO
                    await Data.SaveGeneralAsync();
                    Console.WriteLine("saved");
                }
            }

        }
    }
}

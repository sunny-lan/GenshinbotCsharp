using genshinbot.reactive.wire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class BlackbarFixer
    {
        BotIO Io;

        public BlackbarFixer(BotIO io)
        {
            Io = io;
        }

        public async Task FixBlackBar()
        {
            var sz = await Io.W.Size.Value2();
            var rd = data.db.SettingsScreenDb.Instance.Value.R[sz];

            await Io.K.KeyPress(automation.input.Keys.Escape);
            await Task.Delay(500);

            await Io.M.LeftClick(rd.Settings.RandomWithin());
            await Task.Delay(1000);

            await Io.M.LeftClick(rd.Graphics.RandomWithin());
            await Task.Delay(500);

            await Io.M.LeftClick(rd.DisplayMode.RandomWithin());
            await Task.Delay(500);

            var tmp= Io.W.Size.NonNull().Where(x => x != sz).Get(TimeSpan.FromSeconds(2));
            await Io.M.LeftClick(rd.Windowed.RandomWithin());
            var sz2 = await tmp;

            rd = data.db.SettingsScreenDb.Instance.Value.R[sz2];
            tmp = Io.W.Size.NonNull().Where(x => x == sz).Get(TimeSpan.FromSeconds(2));
            await Io.M.LeftClick(rd.Fullscreen.RandomWithin());
            await tmp;
        }
    }
}

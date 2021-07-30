using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class BlackbarFixer:ITool
    {
        BotIO Io;

        public BlackbarFixer(BotIO io)
        {
            Io = io;
        }
        public async Task<Size> WindowedMode()
        {
            var sz = await Io.W.Size.Value2();
            var rd = data.db.SettingsScreenDb.Instance.Value.R[sz];

            await Io.K.KeyPress(automation.input.Keys.Escape);
            await Task.Delay(5000);

            await Io.M.LeftClick(rd.Settings.RandomWithin());
            await Task.Delay(5000);

            await Io.M.LeftClick(rd.Graphics.RandomWithin());
            await Task.Delay(5000);

            await Io.M.LeftClick(rd.DisplayMode.RandomWithin());
            await Task.Delay(5000);

            var tmp = Io.W.Size.NonNull().Where(x => x != sz).Get(TimeSpan.FromSeconds(5));
            await Io.M.LeftClick(rd.Windowed.RandomWithin());
            var sz2 = await tmp;
            await Task.Delay(5000);

            await Io.M.MouseTo(new Point2d(sz2.Width / 2, -5));
            await Task.Delay(500);
            await Io.M.MouseDown(automation.input.MouseBtn.Left);
            await Task.Delay(200);

            await Io.M.MouseMove(new Point2d(-100, 50));
            await Task.Delay(200);
            await Io.M.MouseUp(automation.input.MouseBtn.Left);
            await Task.Delay(5000);


            return sz2;
        }
        public async Task FullscreenMode()
        {
            var sz2 = await Io.W.Size.Value2();

            var rd = data.db.SettingsScreenDb.Instance.Value.R[sz2];

            await Io.M.LeftClick(rd.DisplayMode.RandomWithin());
            await Task.Delay(5000);

            var tmp = Io.W.Size.NonNull().Where(x => x != sz2).Get(TimeSpan.FromSeconds(5));
            await Io.M.LeftClick(rd.Fullscreen.RandomWithin());
            await tmp;
            await Task.Delay(5000);


            await Io.K.KeyPress(automation.input.Keys.Escape);
            await Task.Delay(5000);
            await Io.K.KeyPress(automation.input.Keys.Escape);
            await Task.Delay(5000);
        }
        public async Task FixBlackBar()
        {
           await WindowedMode();
            await FullscreenMode();
        }
    }
}

using genshinbot.data;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.screens
{

    public class LoadingScreen :IScreen
    {
        public class Db
        {
            public static readonly DbInst<Db> Instance = new DbInst<Db>("screens/loadingScreen.json");
            public class RD
            {
                public Rect Patch { get; set; } = new Rect(10, 10, 20, 20);
            }

            public class Theme
            {
                public string Name { get; set; }
                public Scalar Bg { get; set; }
                public Scalar Fg { get; set; }
            }

            public Theme[] Themes { get; set; } = {
                new Theme
                {
                    Name="dark",
                    Bg=Scalar.FromRgb(r:28,g:28,b:34),
                    Fg=Scalar.FromRgb(r:211,g:188,b:142),
                },
                new Theme
                {
                    Name="light",
                    Bg=Scalar.FromRgb(r:255,g:255,b:255),
                    Fg=Scalar.FromRgb(r:103,g:103,b:103),
                },
            };

            public Dictionary<Size, RD> R { get; set; } = new Dictionary<Size, RD>
            {
                [new Size(1440, 900)] = new RD
                {
                },
                [new Size(1680, 1050)] = new RD
                {
                },
            };
        }



        Mat inrange = new Mat();

        public LoadingScreen(BotIO b, ScreenManager screenManager) : base(b, screenManager)
        {
        }

        ~LoadingScreen()
        {
            inrange.Dispose();
        }


        public async Task WaitTillDone()
        {
            var w = this.Io.W;

            var sz = await w.Size.Value2();
            var patch = db.R[sz].Patch;
            var tmp = await w.Screen.Watch(patch.Center().RectAround(new Size(1, 1))).Get();
            var c = tmp.Value.Mean();

            //check which color theme
            foreach (var t in db.Themes)
            {
                if (t.Bg == c)
                {
                    //wait until cannot detect theme
                    await w.Screen.Watch(patch).Where((m) =>
                    {
                        var subimg = m.Value;
                        Scalar c1 = c;
                        c1.Val0++;
                        c1.Val1++;
                        c1.Val2++;
                        c1.Val3 = 256;
                        Cv2.InRange(subimg, c, c1, inrange);
                        int cnt = inrange.CountNonZero();
                        return cnt < subimg.Width * subimg.Height;
                    }).Get();

                    return;
                }
            }

        }

        Db db = Db.Instance.Value;

    }
}

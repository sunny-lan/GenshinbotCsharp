using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.screens
{

    public class LoadingScreen : Screen
    {
        public class Db
        {
            public class RD
            {
                public Rect Patch { get; internal set; } = new Rect(10, 10, 20, 20);
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


        private GenshinBot b;

        public Db.Theme Theme;
        Mat inrange = new Mat();


        public LoadingScreen(GenshinBot b)
        {
            this.b = b;
        }
        ~LoadingScreen()
        {
            inrange.Dispose();
        }

        public bool CheckActive()
        {
            Theme = findTheme();
            return Theme != null;
        }

        public void WaitTillDone()
        {
            while (findTheme() != null) ;
        }

        Db.Theme findTheme()
        {
            var w = this.b.W;
            var db = this.b.Db.LoadingScreenDb;

            var sz = w.GetSize();
            var patch = db.R[sz].Patch;
            var c = w.GetPixelColor(patch.Left, patch.Top);


            //check which color theme
            foreach (var t in db.Themes)
            {
                if (t.Bg == c)
                {
                    //check all pixels in patch are same color
                    Mat subimg = w.Screenshot(patch);
                    Scalar c1 = c;
                    c1.Val0++;
                    c1.Val1++;
                    c1.Val2++;
                    c1.Val3 = 256;
                    Cv2.InRange(subimg, c, c1, inrange);
                    int cnt = inrange.CountNonZero();
                    if (cnt == subimg.Width * subimg.Height)
                        return t;
                    else
                        return null;//not all pixels matched
                }
            }
            return null;
        }
    }
}

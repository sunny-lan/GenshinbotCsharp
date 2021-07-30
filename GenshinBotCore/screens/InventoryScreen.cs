using genshinbot.data;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.screens
{
    public class InventoryScreen:IScreen
    {
        public enum Tabs
        {
            Weapons,
            Artifacts,
            Talent,
            Food,
            Raw,
            Devices,
            Quest,
            Special,
            Pot
        }
        public class Db
        {
            public static readonly DbInst<Db> Instance = new("screens/InventoryScreen.json");
            public class RD
            {
                public Grid TabButtons { get; set; } = new()
                {
                    Columns = 9,
                    Rows = 1,
                };

                public Grid Items { get; set; } = new();

                public Rect UseButton { get; set; }

                public Snap? BagIcon { get; set; }
            }

            public Dictionary<Size, RD> R { get; set; } = new();
            public double ItemDetectThres { get; set; } = 0.2;
        }

        public async Task SelectTab(Tabs tab)
        {
            var db = Db.Instance.Value;
            var bounds = await Io.W.Bounds.Value2();
            var rd = db.R[bounds.Size];
            await Io.M.LeftClick(rd.TabButtons.Get(0, (int)tab).Smallify(2).Round().RandomWithin());
            await Task.Delay(200);
        }

        Mat templMatch = new();

        public InventoryScreen(BotIO b, ScreenManager s):base(b,s)
        {
        }

        public override IWire<(bool isThis, double score)>? IsCurrentScreen(BotIO b)
        {
            var db = Db.Instance.Value;
            algorithm.BlackWhiteTemplateMatchAlg alg = new() { 
                Preprocess=(a,b)=> { a.CopyTo(b); }, //just use standard template match
            };

            return b.W.Size.Select3<Size, Snap>(sz =>
                 db.R[sz].BagIcon.Expect()).Select3(icon =>
                 {
                     alg.SetTemplate(icon.Image.Value);

                     return b.W.Screen.Watch(icon.Region)
                        .Depacket()
                        .Select(alg.Match);
                 }).Switch2();
        }

        public async Task<(int r, int c,Rect2d bound, double score)> FindItem(Mat item)
        {
            var db = Db.Instance.Value;
            var bounds = await Io.W.Bounds.Value2();
            var rd = db.R[bounds.Size];
            var itemGrid = rd.Items;
            var scrn = await Io.W.Screen.Watch(bounds).Get();
            var res = (0, 0,Rect2d.Empty, score: double.PositiveInfinity);
            for (int r = 0; r < itemGrid.Rows; r++)
            {
                for (int c = 0; c < itemGrid.Columns; c++)
                {
                    var subR = itemGrid.Get(r, c);
                    Cv2.MatchTemplate(scrn[subR.Round()], item, templMatch, TemplateMatchModes.SqDiffNormed);
                    templMatch.MinMaxLoc(out double score, out var _);
                    if (score < res.score)
                    {
                        res = (r, c, subR,score);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Clicks item in current tab
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task SelectItemInCurTab(Mat item)
        {
            var db = Db.Instance.Value;
            var res = await FindItem(item);
            if (res.score > db.ItemDetectThres)
                throw new Exception("Item not found");

            await Io.M.LeftClick(res.bound.RandomWithin());
            await Task.Delay(200);
        }

        public async Task UseCurItem()
        {
            var db = Db.Instance.Value;
            var bounds = await Io.W.Bounds.Value2();
            var rd = db.R[bounds.Size];
            await Io.M.LeftClick(rd.UseButton.RandomWithin());
            await Task.Delay(200);

        }

    }
}

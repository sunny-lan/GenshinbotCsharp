using genshinbot.automation;
using genshinbot.automation.hooking;
using genshinbot.automation.input;
using genshinbot.data;
using genshinbot.reactive;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using static genshinbot.data.GeneralDb;

namespace genshinbot.tools
{
    using RD = Dictionary<Size, Point2d>;
    public static class Dispatch
    {
        private static Folder root = Data.General.Root.FindFolder("dispatch");
        public static RD Hours20 = root.Find("20h").Points;
        public static RD Claim = root.Find("claim").Points;
        public static RD ConfirmRecall = root.Find("confirm_recall").Points;
        public static class Mondstadt
        {
            private static Folder root = Dispatch.root.FindFolder("mondstadt");

            public static RD Button = Dispatch.root.Find("mondstadt").Points;
            public static RD WhisperingWoods = root.Find("whispering_woods").Points;
            public static RD DadupaGorge = root.Find("dadupa_gorge").Points;
            public static RD Wolvendom = root.Find("wolvendom").Points;

            public static RD[] All = { 
                WhisperingWoods,
                DadupaGorge,
                Wolvendom
            };
        }

        public static class Liyue
        {
            private static Folder root = Dispatch.root.FindFolder("liyue");

            public static RD Button = Dispatch.root.Find("liyue").Points;
            public static RD YaoguangShoal = root.Find("yoaguang_shoal").Points;
            public static RD GuyunStoneForest = root.Find("guyun_stone_forest").Points;

            public static RD[] All = {
                YaoguangShoal,
                GuyunStoneForest,
            };
        }

        
    }
   public static class DailyDoer
    {
        static Folder db = Data.General.Root;
        public static async Task Collect(BotIO b, Point button, RD[] all)
        {
            var sz = await b.W.Size.Get();

            await b.M.LeftClick(button);
            await Task.Delay(500);

            foreach (var pt in all)
            {
                await b.M.LeftClick(pt[sz].Round());
                await Task.Delay(500);


                await b.M.LeftClick(Dispatch.Claim[sz].Round());
                await Task.Delay(2000);

                //dismisees the dialogue about what you got 
                await b.M.LeftClick(Dispatch.ConfirmRecall[sz].Round());
                await Task.Delay(500);
            }
        }
        public static async Task CollectMondstadt(BotIO b)
        {
            var sz = await b.W.Size.Get();

            await Collect(b, Dispatch.Mondstadt.Button[sz].Round(), Dispatch.Mondstadt.All);
        }

        public static async Task CollectLiyue(BotIO b)
        {
            var sz = await b.W.Size.Get();

            await Collect(b, Dispatch.Liyue.Button[sz].Round(), Dispatch.Liyue.All);
        }

        public static async Task DispatchChara(BotIO b, Point pos, string chara)
        {
            var sz = await b.W.Size.Get();
            await b.M.LeftClick(pos);
            await Task.Delay(500);

            await b.M.LeftClick(Dispatch.Hours20[sz].Round());
            await Task.Delay(500);

            //select chara
            await b.M.LeftClick(Dispatch.Claim[sz].Round());
            await Task.Delay(500);

            await b.M.LeftClick(db.Find($"character_selector.tmp.{chara}").Points[sz].Round());
            await Task.Delay(500);


        }
        
        public static async Task DispatchMondstadt(BotIO b)
        {
            var sz = await b.W.Size.Get();

            await b.M.LeftClick(Dispatch.Mondstadt.Button[sz].Round());
            await Task.Delay(500);

            await DispatchChara(b, Dispatch.Mondstadt.WhisperingWoods[sz].Round(), "fischl");
            await DispatchChara(b, Dispatch.Mondstadt.DadupaGorge[sz].Round(), "bennett");
            await DispatchChara(b, Dispatch.Mondstadt.Wolvendom[sz].Round(), "amber");
        }

        public static async Task DispatchLiyue(BotIO b)
        {
            var sz = await b.W.Size.Get();

            await b.M.LeftClick(Dispatch.Liyue.Button[sz].Round());
            await Task.Delay(500);

            await DispatchChara(b, Dispatch.Liyue.GuyunStoneForest[sz].Round(), "chongyun");
            await DispatchChara(b, Dispatch.Liyue.YaoguangShoal[sz].Round(), "keqing");
        }

        public static async Task DispatchCollect(BotIO b)
        {
            await ChatBegin(b, "dispatch");

            await CollectLiyue(b);
            await CollectMondstadt(b);
            await DispatchLiyue(b);
            await DispatchMondstadt(b);

            await b.K.KeyPress(Keys.Escape);
            await Task.Delay(2000);
        }

        public static async Task ChatBegin(BotIO b, string name)
        {

            var sz = await b.W.Size.Get();

            await b.K.KeyPress(Keys.F);
            await Task.Delay(2000);

            await b.K.KeyPress(Keys.Space);
            await Task.Delay(500);

            await b.M.LeftClick(db.Find($"tmp.katheryne.{name}").Points[sz].Round());
            await Task.Delay(2000);
        }
        public static async Task GetCommissions(BotIO b)
        {
            var sz = await b.W.Size.Get();

            await ChatBegin(b, "claim_daily_commision");

            await b.K.KeyPress(Keys.Space);
            await Task.Delay(2000);

            await b.M.LeftClick(sz.Center().Round());
            await Task.Delay(2000);
        }
        public static async Task runAsync(BotIO w)
        {

            // await DispatchLiyue(w);
            //await CollectLiyue(w);
            //await DispatchMondstadt(w);
            //await GetCommissions(w);
            await DispatchCollect(w);
        }
    }
}

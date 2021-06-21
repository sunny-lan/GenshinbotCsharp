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

    public class DispatchDb
    {
        public static Lazy<DispatchDb> Instance = new Lazy<DispatchDb>(
            () => Data.ReadJson1<DispatchDb>("dispatchDb.json"));

        public static async Task SaveInstanceAsync(DispatchDb instance=null)
        {
            if (instance == null) instance = Instance.Value;
            await Data.WriteJsonAsync("dispatchDb.json", instance);
        }

        public Dictionary<Size, RD> Rd { get; set; } = new Dictionary<Size, RD>();
        public class RD
        {
            public Point Hours20 { get; set; }
            public Point Claim { get; set; }
            public Point ConfirmRecall { get; set; }
            public Mondstadt Mondstadt { get; set; }
            public Liyue Liyue{ get; set; }
          
        }

        public class Mondstadt
        {

            public Point Button { get; set; }
            public Point WhisperingWoods { get; set; }
            public Point DadupaGorge { get; set; }
            public Point Wolvendom { get; set; }

            public Point[] All
            {
                get
                {
                    return new[]{
                        WhisperingWoods,
                        DadupaGorge,
                        Wolvendom
                   };
                }
            }
        }

        public class Liyue
        {

            public Point Button { get; set; }
            public Point YaoguangShoal { get; set; }
            public Point GuyunStoneForest { get; set; }

            public Point[] All
            {
                get
                {
                    return new[]{
                        YaoguangShoal,
                        GuyunStoneForest,
                    };
                }
            }
        }
    }
    public static class DailyDoer
    {
        static Folder db = Data.General.Root;
        public static async Task Collect(BotIO b, Point button, Point[] all)
        {
            var sz = await b.W.Size.Get();
            var Dispatch = DispatchDb.Instance.Value.Rd[sz];

            await b.M.LeftClick(button);
            await Task.Delay(500);

            foreach (var pt in all)
            {
                await b.M.LeftClick(pt);
                await Task.Delay(500);


                await b.M.LeftClick(Dispatch.Claim);
                await Task.Delay(2000);

                //dismisees the dialogue about what you got 
                await b.M.LeftClick(Dispatch.ConfirmRecall);
                await Task.Delay(500);
            }
        }
        public static async Task CollectMondstadt(BotIO b)
        {
            var sz = await b.W.Size.Get();
            var Dispatch = DispatchDb.Instance.Value.Rd[sz];

            await Collect(b, Dispatch.Mondstadt.Button, Dispatch.Mondstadt.All);
        }

        public static async Task CollectLiyue(BotIO b)
        {
            var sz = await b.W.Size.Get();
            var Dispatch = DispatchDb.Instance.Value.Rd[sz];

            await Collect(b, Dispatch.Liyue.Button, Dispatch.Liyue.All);
        }

        public static async Task DispatchChara(BotIO b, Point pos, string chara)
        {
            var sz = await b.W.Size.Get();
            var Dispatch = DispatchDb.Instance.Value.Rd[sz];

            await b.M.LeftClick(pos);
            await Task.Delay(500);

            await b.M.LeftClick(Dispatch.Hours20);
            await Task.Delay(500);

            //select chara
            await b.M.LeftClick(Dispatch.Claim);
            await Task.Delay(500);

            await b.M.LeftClick(db.Find($"character_selector.tmp.{chara}").Points[sz].Round());
            await Task.Delay(500);


        }

        public static async Task DispatchMondstadt(BotIO b)
        {
            var sz = await b.W.Size.Get();
            var Dispatch = DispatchDb.Instance.Value.Rd[sz];

            await b.M.LeftClick(Dispatch.Mondstadt.Button);
            await Task.Delay(500);

            await DispatchChara(b, Dispatch.Mondstadt.WhisperingWoods, "fischl");
            await DispatchChara(b, Dispatch.Mondstadt.DadupaGorge, "bennett");
            await DispatchChara(b, Dispatch.Mondstadt.Wolvendom, "amber");
        }

        public static async Task DispatchLiyue(BotIO b)
        {
            var sz = await b.W.Size.Get();
            var Dispatch = DispatchDb.Instance.Value.Rd[sz];

            await b.M.LeftClick(Dispatch.Liyue.Button);
            await Task.Delay(500);

            await DispatchChara(b, Dispatch.Liyue.GuyunStoneForest, "chongyun");
            await DispatchChara(b, Dispatch.Liyue.YaoguangShoal, "keqing");
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

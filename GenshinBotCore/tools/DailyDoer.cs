using genshinbot.automation;
using genshinbot.automation.hooking;
using genshinbot.automation.input;
using genshinbot.data;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using static genshinbot.data.GeneralDb;

namespace genshinbot.tools
{
    public class DailyDoer
    {
        BotIO b;

        public DailyDoer(BotIO b)
        {  
            this.b = b;
        }

        KathyrneDb kath=KathyrneDb.Instance.Value;
        CharacterSelectorDb sel=CharacterSelectorDb.Instance.Value;

        public async Task CollectPot()
        {
            var sz = await b.W.Size.Value2();

        }

        public async Task Collect( Point button, Point[] all)
        {
            var sz = await b.W.Size.Value2();
            var Dispatch = DispatchDb.Instance.Rd[sz];

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

        public  async Task DispatchChara( Point pos, string chara)
        {
            var sz = await b.W.Size.Value2();
            var Dispatch = DispatchDb.Instance.Rd[sz];

            await b.M.LeftClick(pos);
            await Task.Delay(500);

            await b.M.LeftClick(Dispatch.Hours20);
            await Task.Delay(500);

            //select chara
            await b.M.LeftClick(Dispatch.Claim);
            await Task.Delay(500);

            await b.M.LeftClick(sel.R[sz].Position[chara].Expect());
            await Task.Delay(500);


        }



        public async Task DispatchAll()
        {
            var sz = await b.W.Size.Value2();
            var Dispatch = DispatchDb.Instance.Rd[sz];

            await b.M.LeftClick(Dispatch.Liyue.Button);
            await Task.Delay(500);

            await DispatchChara(Dispatch.Liyue.YaoguangShoal, "keqing");

            await b.M.LeftClick(Dispatch.Mondstadt.Button);
            await Task.Delay(500);

            await DispatchChara(Dispatch.Mondstadt.WhisperingWoods, "fischl");
            await DispatchChara(Dispatch.Mondstadt.DadupaGorge, "bennett");
            await DispatchChara(Dispatch.Mondstadt.Stormbearer.RandomWithin(), "amber");

            await b.M.LeftClick(Dispatch.Inazuma.Button);
            await Task.Delay(500);

            await DispatchChara(Dispatch.Inazuma.Byakko.RandomWithin(), "chongyun");
        }
        public async Task CollectAll()
        {
            var sz = await b.W.Size.Value2();

            var Dispatch = DispatchDb.Instance.Rd[sz];
            await Collect(Dispatch.Mondstadt.Button, new[]{
                Dispatch.Mondstadt.Stormbearer.RandomWithin(),
                Dispatch.Mondstadt.WhisperingWoods,
                Dispatch.Mondstadt.DadupaGorge,
            });
            await Collect(Dispatch.Liyue.Button, new[]{
                Dispatch.Liyue.YaoguangShoal,
            });
            await Collect(Dispatch.Inazuma.Button, new[]{
                Dispatch.Inazuma.Byakko.RandomWithin(),
            });
        }
        public async Task DispatchCollect()
        {
            var sz = await b.W.Size.Value2();
            await ChatBegin( kath.R[sz].Dispatch.Expect());

            await CollectAll();

            await DispatchAll();

            await b.K.KeyPress(Keys.Escape);
            await Task.Delay(2000);
        }

        public async Task ChatBegin(Point p)
        {
            
            var sz = await b.W.Size.Value2();

            await b.K.KeyPress(Keys.F);
            await Task.Delay(2000);

            await b.K.KeyPress(Keys.Space);
            await Task.Delay(500);

            await b.M.LeftClick(p);
            await Task.Delay(2000);
        }
        public async Task GetCommissions()
        {
            var sz = await b.W.Size.Value2();

            await ChatBegin( kath.R[sz].ClaimDaily.Expect());

            await b.K.KeyPress(Keys.Space);
            await Task.Delay(2000);

            await b.M.LeftClick(sz.Center().Round());
            await Task.Delay(2000);
        }
    }
}

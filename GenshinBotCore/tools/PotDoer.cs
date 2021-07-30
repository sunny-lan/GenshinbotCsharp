using genshinbot.data;
using genshinbot.reactive.wire;
using genshinbot.screens;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class PotDoer:ITool
    {
        public class Db
        {

            public static readonly DbInst<Db> Instance = new("tools/PotDoer.json");

            public class RD
            {
                public SavableMat? PotImage { get; set; } //tmp

            }

            public Dictionary<Size, RD> R { get; set; } = new();


        }
        private readonly InventoryScreen inventory;
        private readonly ScreenManager screenManager;

        public PotDoer(ScreenManager screenManager)
        {
            this.inventory = screenManager.InventoryScreen;
            this.screenManager = screenManager;
        }

        public async Task UsePot()
        {
            await screenManager.ExpectScreen(inventory);
            var sz = await inventory.Io.W.Size.Value2();
            var db = Db.Instance.Value;
            var rd = db.R[sz];
            var pot = rd.PotImage.Expect().Value;
            await inventory.SelectTab(InventoryScreen.Tabs.Devices);
            await inventory.SelectItemInCurTab(pot);
            await inventory.UseCurItem();

        }


    }
}

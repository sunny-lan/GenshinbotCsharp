using genshinbot.controllers;
using genshinbot.data;
using genshinbot.data.map;
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
                public Rect? MapSelector { get; set; }
                public Rect? Teyvat { get; set; }

            }

            public Dictionary<Size, RD> R { get; set; } = new();


        }
        private readonly InventoryScreen inventory;
        private readonly ScreenManager screens;
        private readonly LocationManager locationManager;

        public PotDoer(ScreenManager screenManager , LocationManager locationManager)
        {
            this.inventory = screenManager.InventoryScreen;
            this.screens = screenManager;
            this.locationManager = locationManager;
        }

        public async Task UsePot()
        {
            await screens.ExpectScreen(inventory);
            var sz = await inventory.Io.W.Size.Value2();
            var db = Db.Instance.Value;
            var rd = db.R[sz];
            var pot = rd.PotImage.Expect().Value;
            await inventory.SelectTab(InventoryScreen.Tabs.Devices);
            await inventory.SelectItemInCurTab(pot);
            await inventory.UseCurItem();
            await screens.ExpectScreen(screens.PlayingScreen);

        }
        
        public async Task OpenRealm()
        {
            var curScreen=await screens.FigureScreen();
            if(curScreen!=screens.MapScreen)
            {
                if (curScreen == screens.PlayingScreen)
                    await screens.PlayingScreen.OpenMap();
                else
                    throw new Exception("Unexpected screen");
            }
            await screens.MapScreen.TeleportTo(MapDb.Instance.Value.MondstadtTeleporter);
            await screens.PlayingScreen.OpenInventory();
            
            await UsePot();
            await screens.PlayingScreen.Io.K.KeyPress(automation.input.Keys.F);
            await screens.ExpectScreen(screens.LoadingScreen);
            await screens.LoadingScreen.WaitTillDone();
            //now we are in realm

        }
        public async Task CloseRealm()
        {
            var sz = await inventory.Io.W.Size.Value2();
            var db = Db.Instance.Value;
            var rd = db.R[sz];
            //todo hacky solution
            await screens .ExpectScreen(screens.PlayingScreen);
            await screens.PlayingScreen.Io.K.KeyPress(automation.input.Keys.M);
            await Task.Delay(5000);
            await screens.PlayingScreen.Io.M.LeftClick(rd.MapSelector.Expect().RandomWithin());
            await Task.Delay(2000);
            await screens.PlayingScreen.Io.M.LeftClick(rd.Teyvat.Expect().RandomWithin());
            await Task.Delay(2000);
            await screens.PlayingScreen.Io.K.KeyPress(automation.input.Keys.Escape); //close side menu
            await Task.Delay(2000);
            await screens.ExpectScreen(screens.MapScreen);
            await Task.Delay(7000);
            await screens.MapScreen.TeleportTo(MapDb.Instance.Value.MondstadtTeleporter);
        }
    }
}

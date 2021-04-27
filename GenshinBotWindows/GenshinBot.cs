
using genshinbot.automation;
using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot
{
    public class GenshinBot
    {
        #region Database
        public class Database
        {
            private Lazy<database.map.MapDb> mapDb = new Lazy<database.map.MapDb>(
                () => Data.ReadJson("map/db.json", database.map.MapDb.Default()));
            public database.map.MapDb MapDb => mapDb.Value;
            public screens.LoadingScreen.Db LoadingScreenDb { get; } = new screens.LoadingScreen.Db();
            public screens.MapScreen.Db MapScreenDb { get; } = new screens.MapScreen.Db();
        }

        public Database Db;

        public void InitDb()
        {
            Debug.Assert(Db == null);

            Db = new Database();
            Console.WriteLine("Database load finish");
        }

        #endregion

        #region Screens

        public screens.PlayingScreen PlayingScreen;
        public screens.MapScreen MapScreen;
        public screens.LoadingScreen LoadingScreen;

        bool screensInit = false;
        public void InitScreens()
        {
            Debug.Assert(!screensInit);
            Debug.Assert(Db != null);

           //TODO PlayingScreen = new screens.PlayingScreen(this);
            MapScreen = new screens.MapScreen(this);
            LoadingScreen = new screens.LoadingScreen(this);

            Console.WriteLine("Screens initialized");
            screensInit = true;
        }

        #endregion

        #region UI

        public YUI Ui;
        genshinbot.tools.BotControlPanel scriptListUi = new genshinbot.tools.BotControlPanel();
        public void InitUi()
        {
            Debug.Assert(Ui == null);

            Ui = yui.windows.MainForm.make();

            Load(scriptListUi);

            Console.WriteLine("UI initialized");
        }

        #endregion

        #region Automation

        public IWindowAutomator W;
        public input.MouseMover M;
        public input.GenshinKeymap K;

        public event EventHandler<bool> AttachedWindowChanged;
        public void AttachWindow()
        {
            Debug.Assert(Db != null);
            Debug.Assert(W == null);

            Console.WriteLine("Attaching to window");

            W = GenshinWindow.FindExisting();
            M = new input.MouseMover(W);
            K = new input.GenshinKeymap(W);

            Console.WriteLine("Genshin window initialized");

            AttachedWindowChanged?.Invoke(this, true);
        }

        #endregion

        #region Controllers

        public controllers.LocationManager LocationManager;

        public void InitControllers()
        {
            Debug.Assert(LocationManager == null);
            Debug.Assert(Db != null);
            Debug.Assert(screensInit);


            LocationManager = new controllers.LocationManager(this);
            Console.WriteLine("Controllers initialized");
        }

        #endregion

        public async Task ParallelInitAll()
        {
            Console.WriteLine("Bot init begin");
            Task initDb = Task.Run(InitDb);
            Task initUi = Task.Run(InitUi);
            Task initScreens = initDb.ContinueWith(_ => InitScreens());
            Task initControllers = initScreens.ContinueWith(_ => InitControllers());
            await Task.WhenAll(initDb, initUi, initScreens, initControllers);
            Console.WriteLine("Bot init done");
        }

        #region Scripts

        HashSet<Script> loadedScripts = new HashSet<Script>();

        public bool IsLoaded(Script s)
        {
            return loadedScripts.Contains(s);
        }
        public void Load(Script s)
        {
            if (loadedScripts.Contains(s))
                throw new Exception("Script already loaded");

            s.Load(this);
            loadedScripts.Add(s);
        }

        public void Unload(Script s)
        {
            if (!loadedScripts.Remove(s))
                throw new Exception("Script not loaded");

            s.Unload(this);
        }

        #endregion

        public static void generalTest()
        {

            GenshinBot b = new GenshinBot();
            b.ParallelInitAll().Wait();
            var waiter = EventWaiter.Waiter<object>();
            b.Ui.OnClose = () => { waiter.Item2(null); return true; };
            waiter.Item1.Wait();
        }
    }
}

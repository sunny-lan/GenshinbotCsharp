
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    class GenshinBot
    {
        #region Database
        public class Database
        {
            private Lazy<database.map.MapDb> mapDb = new Lazy<database.map.MapDb>(() => Data.ReadJson("map/db.json", database.map.MapDb.Default()));
            public database.map.MapDb MapDb=>mapDb.Value;  

            public screens.PlayingScreen.Db PlayingScreenDb { get; } = new screens.PlayingScreen.Db();
            public screens.LoadingScreen.Db LoadingScreenDb { get;} = new screens.LoadingScreen.Db();
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

        public Screen ActiveScreen;
        public screens.PlayingScreen PlayingScreen;
        public screens.MapScreen MapScreen;
        public screens.LoadingScreen LoadingScreen;

        public bool SIs<T>() where T : Screen
        {
            return ActiveScreen is T;
        }

        /// <summary>
        /// expects the current active screen to be T, and returns it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T S<T>() where T:Screen
        {
            if (ActiveScreen is T t)
                return t;
            throw new Exception("Tried to access a screen which is inactive");
        }

        /// <summary>
        /// sets the active screen to the given one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>

        public T S<T>(T s) where T : Screen
        {
            //TODO
           // if(!s.CheckActive())
           //     throw new Exception("Tried to give control to a screen which is inactive");
            ActiveScreen = s;
            return s;
        }

        /// <summary>
        /// waits for a screen to actually become active, then sets current active screen to that
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public T SWait<T>(T s) where T : Screen
        {
            while (!s.CheckActive()) ;
            return S(s);
        }

        bool screensInit = false;
        public void InitScreens()
        {
            Debug.Assert(!screensInit);
            Debug.Assert(Db != null);

            PlayingScreen = new screens.PlayingScreen(this);
            MapScreen = new screens.MapScreen(this);
            LoadingScreen = new screens.LoadingScreen(this);

            Console.WriteLine("Screens initialized");
            screensInit = true;
        }

        #endregion

        #region UI

        public YUI Ui;
        public void InitUi()
        {
            Debug.Assert(Ui == null);

            Ui = yui.WindowsForms.MainForm.make();
            Console.WriteLine("UI initialized");
        }

        #endregion

        #region Automation

        public GenshinWindow W;
        public input.MouseMover M;
        public void AttachWindow()
        {
            Debug.Assert(Db != null);
            Debug.Assert(W == null);

            Console.WriteLine("Attaching to window");

            W = GenshinWindow.FindExisting();
            M = new input.MouseMover(W);

            Console.WriteLine("Genshin window initialized");
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
            Task initScreens = initDb.ContinueWith(_=>InitScreens());
            Task initControllers = initScreens.ContinueWith(_=>InitControllers());
            await Task.WhenAll(initDb, initUi, initScreens, initControllers);
            Console.WriteLine("Bot init done");
        }

        public static void generalTest()
        {
            GenshinBot b = new GenshinBot();
            b.ParallelInitAll().Wait();
            b.AttachWindow();
            while (true) Task.Delay(10000).Wait();
        }
    }
}

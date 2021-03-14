using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    class GenshinBot
    {
        public database.Database Db;

        public GenshinWindow W;
        public input.MouseMover M;

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

        public controllers.LocationManager LocationManager;

        

        public GenshinBot()
        {
            Console.WriteLine("Bot load begin");

            //TODO implement parallel loading
            Db = new database.Database
            {
                MapDb = Data.ReadJson("map/db.json", database.map.MapDb.Default()),
                LocationManagerDb = Data.ReadJson("controllers/LocationManager.json", database.controllers.LocationManagerDb.Default())
            };

            Console.WriteLine("Database load finish");

            W = GenshinWindow.FindExisting();
            M = new input.MouseMover(W);

            Console.WriteLine("Genshin window initialized");

            PlayingScreen = new screens.PlayingScreen(this);
            MapScreen = new screens.MapScreen(this);
            LoadingScreen = new screens.LoadingScreen(W);

            Console.WriteLine("Screens initialized");

            //LocationManager = new controllers.LocationManager(this);

            Console.WriteLine("Controllers initialized");
            Console.WriteLine("Bot initialized");
        }
    }
}

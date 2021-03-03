using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    class GenshinBot
    {
        //always intialized
        public GenshinWindow W;
        public database.Database Db;

        public Screen ActiveScreen;
        public screens.PlayingScreen PlayingScreen;
        public screens.MapScreen MapScreen;

        public bool SIs<T>() where T : Screen
        {
            return ActiveScreen is T;
        }
        public T S<T>() where T:Screen
        {
            if (ActiveScreen is T t)
                return t;
            throw new Exception("Tried to access a screen which is inactive");
        }

        public T S<T>(T s) where T : Screen
        {
            //TODO
           // if(!s.CheckActive())
           //     throw new Exception("Tried to give control to a screen which is inactive");
            ActiveScreen = s;
            return s;
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

            Console.WriteLine("Genshin window initialized");

            PlayingScreen = new screens.PlayingScreen(this);
            MapScreen = new screens.MapScreen(this);

            Console.WriteLine("Screens initialized");

            LocationManager = new controllers.LocationManager(this);

            Console.WriteLine("Controllers initialized");
            Console.WriteLine("Bot initialized");
        }
    }
}

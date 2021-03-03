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

        public T S<T>() where T:Screen
        {
            if (ActiveScreen is T t)
                return t;
            throw new Exception("Tried to control a screen which is inactive");
        }

        public T S<T>(T s) where T : Screen
        {
            if(!s.CheckActive())
                throw new Exception("Tried to control a screen which is inactive");
            ActiveScreen = s;
            return s;
        }


        public controllers.LocationManager LocationManager;

        

        public GenshinBot()
        {
            Db = new database.Database
            {
                MapDb = Data.ReadJson("map/db.json", database.map.MapDb.Default()),
                LocationManagerDb = Data.ReadJson("controllers/LocationManager.json", database.controllers.LocationManagerDb.Default())
            };

            W = GenshinWindow.FindExisting();

            PlayingScreen = new screens.PlayingScreen(this);
            MapScreen = new screens.MapScreen(this);

            LocationManager = new controllers.LocationManager(this);
        }

    }
}

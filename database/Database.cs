using GenshinbotCsharp.database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.database
{
    /// <summary>
    /// A POCO containing all state and configuration information for the bot
    /// No "heavy" fields such as images are allowed
    /// Instead store that data to a separate file and store the path
    /// For reference look at database.Image
    /// </summary>
    class Database
    {
        public map.MapDb MapDb { get; set; }
        public controllers.LocationManager.Db LocationManagerDb { get; set; }

        public screens.PlayingScreen.Db PlayingScreenDb { get; set; } = new screens.PlayingScreen.Db();
    }
}

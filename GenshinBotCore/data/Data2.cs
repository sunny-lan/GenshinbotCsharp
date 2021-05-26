using System;
using System.Collections.Generic;
using System.Text;

namespace genshinbot.data
{
    /// <summary>
    /// For sake of simplicity, store database in global thing
    /// </summary>
    static partial class Data
    {
        private static Lazy<map.MapDb> mapDb = new Lazy<map.MapDb>(
                 () => ReadJson("map/db.json", map.MapDb.Default()));
        public static map.MapDb MapDb => mapDb.Value;

        private static Lazy<ClickDb> clickDb = new Lazy<ClickDb>(
                () => ReadJson1<ClickDb>("clickDb.json"));
        public static ClickDb ClickDb=>clickDb.Value;
    }
}

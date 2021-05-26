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

        private static Lazy<GeneralDb> clickDb = new Lazy<GeneralDb>(
                () => ReadJson1<GeneralDb>("clickDb.json"));
        public static GeneralDb ClickDb=>clickDb.Value;
    }
}

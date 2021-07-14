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
        //todo refactor
        public static map.MapDb MapDb => map.MapDb.Instance.Value;

        private static Lazy<GeneralDb> general = new Lazy<GeneralDb>(
                () => ReadJson1<GeneralDb>("generaldb.json"));
        public static GeneralDb General=>general.Value;

        //TODO
        public static async System.Threading.Tasks.Task SaveGeneralAsync()
        {
            await WriteJsonAsync("generaldb.json",General);
        }
    }
}

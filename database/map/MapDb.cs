using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.database.map
{
    class MapDb
    {
        public List<Feature> Features { get; set; }
        public static MapDb Default() {
            return new MapDb
            {
                Features = new List<Feature>(),
            };
        }

    }
}

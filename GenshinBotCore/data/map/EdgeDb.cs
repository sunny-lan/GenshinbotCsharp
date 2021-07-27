using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data.map
{
    public class EdgeDb
    {
        public static readonly DbInst<EdgeDb> Instance = new DbInst<EdgeDb>(
            "map/edges.json",
            EdgeDb.MigrateFrom(map.MapDb.Instance.Value)
        );
        public class Edge
        {
            public int Src { get; set; }
            public int Dst { get; set; }

            public bool? ExpectClimb { get; set; }
            public bool? ExpectFly { get; set; }
        }

        public List<Edge> Edges { get; set; } = new List<Edge>();

        public static EdgeDb MigrateFrom(MapDb mapDb)
        {
            var res = new EdgeDb();
            foreach(var f in mapDb.Features)
            {
                if (f.Reachable is null) continue;
                foreach(var neigh in f.Reachable)
                {
                    res.Edges.Add(new Edge {
                        Src=f.ID,
                        Dst=neigh
                    });
                }
            }
            return res;
        }
    }
}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data.db
{
    public record Inventory
    {
        public static readonly DbInst<Inventory> Instance = new("screens/inventory.json");
        public record Tab
        {
            public Point Btn { get; set; }
        }
        public record Devices :Tab
        {
            public Point Pot { get; set; }
        }
        public record RD
        {
            public Devices? DevicesTab { get; set; }
            public Point Use { get; set; }
        }
        public Dictionary<Size, RD> R { get; set; } = new();
    }
}

using OpenCvSharp;
using System.Collections.Generic;

namespace genshinbot.data
{
    public class KathyrneDb
    {
        public static readonly DbInst<KathyrneDb> Instance=new ("kathyrne.json");
        public class RD
        {
            public Point? Dispatch { get; set; }
            public Point? ClaimDaily { get; set; }
        }

        public Dictionary<Size, RD> R { get; set; } = new();

    }
}

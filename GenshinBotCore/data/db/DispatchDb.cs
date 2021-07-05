using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace genshinbot.data
{
    public class DispatchDb
    {
        public static DispatchDb Instance => inst.Value;
        public static Lazy<DispatchDb> inst = new Lazy<DispatchDb>(
            () => Data.ReadJson1<DispatchDb>("dispatchDb.json"));

        public static async Task SaveInstanceAsync(DispatchDb instance = null)
        { 
            if (instance == null) instance = Instance;
            await Data.WriteJsonAsync("dispatchDb.json", instance);
        }

        public Dictionary<Size, RD> Rd { get; set; } = new Dictionary<Size, RD>();
        public class RD
        {
            public Point Hours20 { get; set; }
            public Point Claim { get; set; }
            public Point ConfirmRecall { get; set; }
            public Mondstadt Mondstadt { get; set; }
            public Liyue Liyue { get; set; }

        }

        public class Mondstadt
        {

            public Point Button { get; set; }
            public Point WhisperingWoods { get; set; }
            public Point DadupaGorge { get; set; }
            public Point Wolvendom { get; set; }

            public Point[] All
            {
                get
                {
                    return new[]{
                        WhisperingWoods,
                        DadupaGorge,
                        Wolvendom
                   };
                }
            }
        }

        public class Liyue
        {

            public Point Button { get; set; }
            public Point YaoguangShoal { get; set; }
            public Point GuyunStoneForest { get; set; }

            public Point[] All
            {
                get
                {
                    return new[]{
                        YaoguangShoal,
                        GuyunStoneForest,
                    };
                }
            }
        }
    }
}

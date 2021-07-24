using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace genshinbot.data
{
    public class DispatchDb
    {
        public static DispatchDb Instance => inst.Value;
        public static DbInst<DispatchDb> inst = new("dispatchDb.json");


        public Dictionary<Size, RD> Rd { get; set; } = new Dictionary<Size, RD>();
        public class RD
        {
            public Point Hours20 { get; set; }
            public Point Claim { get; set; }
            public Point ConfirmRecall { get; set; }
            public Mondstadt Mondstadt { get; set; }
            public Liyue Liyue { get; set; }
            public Inazuma Inazuma { get; set; }

        }
        public class City
        {
            public Point Button { get; set; }

        }
        public class Mondstadt:City
        {

            public Point WhisperingWoods { get; set; }
            public Point DadupaGorge { get; set; }
            public Point Wolvendom { get; set; }
            public Rect Stormbearer { get; set; }
            public Rect Windrise { get; set; }
            public Rect Stormterror { get; set; }

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

        public class Liyue:City
        {

            public Point YaoguangShoal { get; set; }
            public Point GuyunStoneForest { get; set; }
            public Rect Jueyun { get; set; }
            public Rect Dunyu { get; set; }
            public Rect Guili { get; set; }
            public Rect Dihua { get; set; }

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


        public class Inazuma:City
        {

            public Rect Jinren { get; set; }
            public Rect Byakko { get; set; }
            public Rect Konda { get; set; }

            public Rect[] All
            {
                get
                {
                    return new[]{
                        Jinren,
                        Byakko,
                        Konda,
                    };
                }
            }
        }
    }
}

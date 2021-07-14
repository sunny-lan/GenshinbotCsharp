using genshinbot.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.screens
{
    public class Quests : IScreen
    {
        public class Db
        {
            public static readonly DbInst<Db> Instance = new DbInst<Db>("screens/quests.json");
            public abstract class Tab
            {
                public Rect Btn { get; set; }
            }

            public class DailiesTab:Tab
            {
                public Rect[] DailyBtns { get; set; } = new Rect[4];

            }

            public class RD
            {
                public DailiesTab Dailies { get; set; } = new DailiesTab();
                public Rect QuestTitle { get; set; }
                public Rect Navigate { get; set; }
                public Rect QuestLocation { get; set; }
                public Rect QuestDescription { get; set; }
            }

            public Dictionary<Size, RD> R { get; set; } = new Dictionary<Size, RD>();
        }
        Db db = Db.Instance.Value;
        public Quests(BotIO b, ScreenManager screenManager) : base(b, screenManager)
        {

        }

        
    }
}

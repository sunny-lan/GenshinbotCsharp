using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data.db
{
   public class IconDb
    {
        public static readonly DbInst<IconDb> Instance = new("icons.json");
    
        public class Icon
        {
            public string Name { get; set; }

            public Icon(string name)
            {
                Name = name;
                Image = new($"icon_{name}");
            }

            public SavableMat Image { get; set; }

        }

        public class RD {
            public Icon Chat { get; set; } = new("chat");
            
        }
        public Dictionary<Size, RD> R = new();
    }
}

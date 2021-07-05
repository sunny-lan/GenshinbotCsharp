using OpenCvSharp;
using System.Collections.Generic;

namespace genshinbot.data
{
    public class CharacterSelectorDb
    {

        public static readonly DbInst<CharacterSelectorDb> Instance = new("characterSelect.json");
        public class RD
        {
            public Dictionary<string, Point?> Position { get; init; } = new() { 
                ["fischl"]=null,
                ["amber"]=null,
                ["bennett"]=null,
                ["keqing"]=null,
                ["chongyun"]=null,

            };
        }

        public Dictionary<Size, RD> R { get; set; } = new();
    }
}

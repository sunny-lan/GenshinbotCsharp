using OpenCvSharp;
using System.Text.Json.Serialization;

namespace genshinbot.data
{
    public class SavableMatSubset
    {
        public string Path { get; set; }
        public ImreadModes ImreadMode { get; set; } = ImreadModes.Unchanged;
    }
    public class SavableMat : SavableMatSubset
    {
        public Mat Value { get; set; }
    }
}
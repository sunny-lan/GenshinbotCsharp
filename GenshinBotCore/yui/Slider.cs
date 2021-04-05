using System;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public interface Slider
    {
        public int V { get; set; }
        public event Action<int> VChanged;
        public int Max { get; set; }
        public int Min { get; set; }
        public string Label { get; set; }
        public bool Enabled { get; set; }
    }
}

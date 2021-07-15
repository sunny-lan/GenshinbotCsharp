using System;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public interface Textbox
    {
        public bool Enabled { get; set; }
        public string Text { get; set; }
        public event Action<string> TextChanged;
    }
}

using System;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public interface Button:Deletable
    {
        event EventHandler Click;
        string Text { get; set; }

        bool Enabled { get; set; }
    }
}

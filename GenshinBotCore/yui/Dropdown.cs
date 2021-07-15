using System;
using System.Collections.Generic;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public interface Dropdown:Enablable
    {
        public int Selected { get; set; }
        public List<string> Options { get; set; }
        public event Action<int> OptionSelected;
    }
}

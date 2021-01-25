using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace GenshinbotCsharp
{
    class KeyboardStateTracker
    {
        Dictionary<VirtualKeyCode, bool> dict = new Dictionary<VirtualKeyCode, bool>();

        public void OnEvent(KeyboardEvent evt)
        {
            dict[evt.KeyCode] = evt.KbType == KeyboardEvent.KbEvtType.DOWN;
        }

        public bool IsDown(VirtualKeyCode key)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            else
                return false;
        }
    }
}

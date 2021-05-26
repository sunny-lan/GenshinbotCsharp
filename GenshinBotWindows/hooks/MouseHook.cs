using genshinbot.automation.hooking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace genshinbot.hooks
{
    public class MouseHook: BasicWindowsHookEx<User32.MOUSEHOOKSTRUCT>
    {
        public MouseHook() : base(User32.HookType.WH_MOUSE_LL) { }

    }
}

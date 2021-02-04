using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace GenshinbotCsharp.hooks
{
    class KbdHooker : BasicHooker<User32.KBDLLHOOKSTRUCT>
    {
        public KbdHooker() : base(User32.HookType.WH_KEYBOARD_LL) { }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace GenshinbotCsharp.hooks
{
    class WindowEvtHook : BasicHooker<User32.CWPSTRUCT>
    {
        public WindowEvtHook() : base(User32.HookType.WH_CALLWNDPROC) { }
    }
}

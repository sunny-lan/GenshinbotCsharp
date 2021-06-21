using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace genshinbot.hooks
{
    struct WinEvent
    {
        public uint type;
        public HWND hwnd;
        public int idObject;
        public int idChild;
        public uint idEventThread;
        public uint dwmsEventTime;
    }
    /// <summary>
    /// Provides hooking using SetWinEventHook
    /// </summary>
    class WinEventHook : HookBase<WinEvent>
    {
        private User32.HWINEVENTHOOK hookID;
        private uint processInterest;
        private uint threadInterest;
        private uint eventRangeMin;
        private uint eventRangeMax;

        public WinEventHook(
            uint eventRangeMin = User32.EventConstants.EVENT_MIN,
            uint eventRangeMax = User32.EventConstants.EVENT_MAX,
            uint processOfInterest = 0,
            uint threadOfInterest = 0
            )
        {
            this.processInterest = processOfInterest;
            this.threadInterest = threadOfInterest;
            this.eventRangeMin = eventRangeMin;
            this.eventRangeMax = eventRangeMax;
        }

        protected override void cleanup()
        {
            if (!User32.UnhookWinEvent(hookID))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        protected override object init()
        {
            User32.WinEventProc del = HandleWinEvent;
            hookID = User32.SetWinEventHook(
                eventRangeMin, eventRangeMax,
                HINSTANCE.NULL,
                del,
                processInterest, threadInterest,
                User32.WINEVENT.WINEVENT_OUTOFCONTEXT
            );
            if (hookID.IsNull)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return del;

        }

        private void HandleWinEvent(User32.HWINEVENTHOOK hWinEventHook, uint winEvent, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
        {
            signal(new WinEvent
            {
                type = winEvent,
                hwnd = hwnd,
                idChild = idChild,
                idObject = idObject,
                idEventThread = idEventThread,
                dwmsEventTime = dwmsEventTime,
            });
        }

        public static void Test()
        {
            var consts = GetConstants(typeof(User32.ObjectIdentifiers));
            var objId = new Dictionary<int, string>();
            foreach (var x in consts)
            {
                if (x.FieldType == typeof(int))
                    objId[(int)x.GetRawConstantValue()] = x.Name;
            }

            consts = GetConstants(typeof(User32.EventConstants));
            var evtType = new Dictionary<uint, string>();
            foreach (var x in consts)
            {
                if (x.FieldType == typeof(uint))
                    evtType[(uint)x.GetRawConstantValue()] = x.Name;
            }

            //var hwnd = User32.FindWindow( "UnityWndClass", "Genshin Impact");//(null, "Untitled - Notepad");//
            var hwnd = User32.FindWindow(null, "*Untitled - Notepad");//
            User32.GetWindowThreadProcessId(hwnd, out uint pid);
            var hook = new WinEventHook(processOfInterest: pid);
            var hook2 = new WinEventHook(eventRangeMax:User32.EventConstants.EVENT_SYSTEM_FOREGROUND, eventRangeMin:User32.EventConstants.EVENT_SYSTEM_FOREGROUND);
            hook.Start();
            Thread.Sleep(1000);
            hook.Stop();
            Console.WriteLine("ready");
            hook.Start();
            hook.OnEvent += ( e) =>
            {
                //if (e.hwnd == hwnd) Console.WriteLine("genshin open");
                //else Console.WriteLine("genshin close");
                if(e.idObject==User32.ObjectIdentifiers.OBJID_WINDOW)
                Console.WriteLine(" {0:x} {1:x}", get(objId, e.idObject), get(evtType, e.type));
            };
        }

        private static object get<K,T>(Dictionary<K,T> d, K k)
        {
            if (d.ContainsKey(k)) return d[k];
            return k;
        }

        private static List<FieldInfo> GetConstants(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                 BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
        }
    }
}

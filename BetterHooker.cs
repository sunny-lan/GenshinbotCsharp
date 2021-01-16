using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace GenshinbotCsharp
{

    class BetterHooker : IDisposable //TODO
    {
        private int hookID;
        private uint threadID;
        private const uint CUST_MSG = 0x666;
        private WinAPI.HookProc proc;

        static IntPtr mar = WinAPI.LoadLibrary("user32.dll");

        private const int WM_QUIT = 0x12;

        private Queue<Event> records;
        public BetterHooker()
        {
            records = new Queue<Event>();
            threadID = WinAPI.GetCurrentThreadId();
            proc = KeyboardHookProc;
            hookID = WinAPI.SetWindowsHookEx(
                    WinAPI.WH_KEYBOARD_LL,
                    proc,
                    mar,
                    0);
            if (hookID == 0)
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();

                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }
        }

        private static KeyboardEvent KbdToEvent(int wParam, IntPtr lParam)
        {
            var keyStruct = (WinAPI.KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(WinAPI.KeyboardHookStruct));
            KeyboardEvent r = new KeyboardEvent();
            r.KeyCode = (WindowsInput.Native.VirtualKeyCode)keyStruct.VirtualKeyCode;
            if (wParam == WinAPI.WM_KEYUP) r.KbType = KeyboardEvent.KbEvtType.UP;
            else if (wParam == WinAPI.WM_KEYDOWN) r.KbType = KeyboardEvent.KbEvtType.DOWN;
            else r = null;
            return r;
        }

        private int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (WinAPI.GetCurrentThreadId() != threadID)
                throw new Exception("Cross thread call");



            if (nCode >= 0)
            {
                var r = KbdToEvent(wParam, lParam);
                if (r != null)
                {
                    records.Enqueue(r);
                    if (!WinAPI.PostThreadMessage(threadID, CUST_MSG, UIntPtr.Zero, IntPtr.Zero))
                    {
                        throw new Exception("Failed to post message");
                    }
                }
            }


            //forward to other application
            return WinAPI.CallNextHookEx(hookID, nCode, wParam, lParam);
        }



        public Event WaitEvent()
        {
            if (WinAPI.GetCurrentThreadId() != threadID)
                throw new Exception("Cross thread call");

            var msg = new MSG();

            while (true)
            {
                int ret = WinAPI.GetMessage(ref msg, 0, 0, 0);
                if (ret == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (ret == 0)
                    throw new Exception("hooker disposed");

                if (msg.message == CUST_MSG)
                    return records.Dequeue();


                WinAPI.TranslateMessage(ref msg);
                WinAPI.DispatchMessage(ref msg);

            }

        }

        private void Interrupt()
        {
            WinAPI.PostThreadMessage(threadID, WM_QUIT, UIntPtr.Zero, IntPtr.Zero);
        }



        public void Dispose()
        {
            if (hookID != 0)
            {
                //uninstall hook
                var result = WinAPI.UnhookWindowsHookEx(hookID);
                //reset invalid handle
                hookID = 0;
                //if failed and exception must be thrown
                if (result == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    var errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
            //Interrupt();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace GenshinbotCsharp
{

    class BetterHooker : IDisposable //TODO
, IHooker
    {
        private User32.SafeHHOOK hookID;
        private uint threadID;
        private const uint CUST_MSG = 0x666, TIMEOUT_MSG=0x667;
        private User32.HookProc proc;

        static Kernel32.SafeHINSTANCE mar = Kernel32.LoadLibrary("user32.dll");

        private const int WM_QUIT = 0x12;

        private Queue<Event> records;

        //public event System.EventHandler<Event> OnEvent;
        public BetterHooker()
        {
            records = new Queue<Event>();
            threadID = Kernel32.GetCurrentThreadId();
            proc = KeyboardHookProc;
            hookID = User32.SetWindowsHookEx(
                    User32.HookType.WH_KEYBOARD_LL,
                    proc,
                    mar,
                    0);
            if (hookID.IsInvalid)
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();

                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }
        }

        private static KeyboardEvent KbdToEvent(IntPtr wParam, IntPtr lParam)
        {
            var keyStruct = (User32.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(User32.KBDLLHOOKSTRUCT));
            KeyboardEvent r = new KeyboardEvent();
            r.KeyCode = (WindowsInput.Native.VirtualKeyCode)keyStruct.vkCode;
            if ((User32.WindowMessage)wParam == User32.WindowMessage.WM_KEYUP) r.KbType = KeyboardEvent.KbEvtType.UP;
            else if ((User32.WindowMessage)wParam == User32.WindowMessage.WM_KEYDOWN) r.KbType = KeyboardEvent.KbEvtType.DOWN;
            else r = null;
            return r;
        }

        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (Kernel32.GetCurrentThreadId() != threadID)
                throw new Exception("Cross thread call");



            if (nCode >= 0)
            {
                var r = KbdToEvent(wParam, lParam);
                if (r != null)
                {
                    records.Enqueue(r);
                    if (!User32.PostThreadMessage(threadID, CUST_MSG, IntPtr.Zero, IntPtr.Zero))
                    {
                        throw new Exception("Failed to post message");
                    }
                }
            }


            //forward to other application
            return User32.CallNextHookEx(hookID, nCode, wParam, lParam);
        }


        //TODO implement timeout
        public Event WaitEvent()
        {
            if (Kernel32.GetCurrentThreadId() != threadID)
                throw new Exception("Cross thread call");

          

            while (true)
            {
                MSG msg;
                if (!User32.GetMessage(out msg, HWND.NULL, 0, 0))
                    throw new Exception("hooker disposed");

                if (msg.message == CUST_MSG)
                    return records.Dequeue();

                else if (msg.message == TIMEOUT_MSG)
                    return null;


                User32.TranslateMessage(msg);
                User32.DispatchMessage(msg);

            }

        }

        private void Interrupt()
        {
            User32.PostThreadMessage(threadID, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        }



        public void Dispose()
        {


            //if failed and exception must be thrown
            if (!User32.UnhookWindowsHookEx(hookID))
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();
                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }
            //Interrupt();
        }
    }
}

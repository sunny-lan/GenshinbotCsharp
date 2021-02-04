using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace GenshinbotCsharp.hooks
{
    abstract class BasicHooker<T>:Hooker<T> where T : struct
    {
        protected BasicHooker(User32.HookType hooktype):base(hooktype)
        {
            
        }

        protected override void HookProc(IntPtr wParam, IntPtr lParam)
        {
            signal(Marshal.PtrToStructure<T>(lParam));
        }
    }

     abstract class Hooker<T> where T:struct
    {

        private User32.SafeHHOOK hookID;
        private uint threadID;

        static Kernel32.SafeHINSTANCE mar = Kernel32.LoadLibrary("user32.dll");

        private const int WM_QUIT = 0x12;

        private object lck=new object();
        private Thread loopThread;
        private T value;


        private User32.HookType HookType;
        protected Hooker(User32.HookType hooktype)
        {
            HookType = hooktype;
            loopThread = new Thread(loop);
            loopThread.Start();
        }

        private IntPtr HookProc1(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (Kernel32.GetCurrentThreadId() != threadID)
                throw new Exception("Cross thread call");

            if (nCode >= 0)
            {
                HookProc(wParam, lParam);
            }

            return User32.CallNextHookEx(hookID, nCode, wParam, lParam);
        }
        private void loop()
        {
            threadID = Kernel32.GetCurrentThreadId();
            hookID = User32.SetWindowsHookEx(
                    HookType,
                    HookProc1,
                    mar,
                    0);
            if (hookID.IsInvalid)
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();

                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }

            while (true)
            {
                MSG msg;
                if (!User32.GetMessage(out msg, HWND.NULL, 0, 0))
                    break;

                User32.TranslateMessage(msg);
                User32.DispatchMessage(msg);
            }

            if (!User32.UnhookWindowsHookEx(hookID))
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();
                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }
        }

        //blocks until a event is recieved
        //events shall not be written to
        //thread safe
        public T WaitEvent(int timeout)
        {
            lock (lck)
            {
                Monitor.Wait(lck);//wait for value to be set
                return value;
            }
        }

        //once signaled, the event object instance shall not be accessed again by the hooker
        protected void signal(T evt){
            lock (lck)
            {
                value = evt;
                Monitor.PulseAll(lck);

            }
            
        }

        protected abstract void HookProc(IntPtr wParam, IntPtr lParam);

        private void Interrupt()
        {
            User32.PostThreadMessage(threadID, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            
            User32.UnhookWindowsHookEx(hookID);
        }



        ~Hooker()
        {
            Interrupt();
            loopThread.Join();
        }
    }
}

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

    /// <summary>
    /// Provides a base implementation with a message loop for all hooking classes
    /// </summary>
    /// <typeparam name="T">The event type returned by the hook</typeparam>
    abstract class HookBase<T> 
    {
        protected uint ThreadID { get; private set; }

        private const int WM_QUIT = 0x12;

        private Thread loopThread;

        /// <summary>
        /// Note: this event will be raised on the message loop thread
        /// </summary>
        public event EventHandler<T> OnEvent;

        public bool Running { get; private set; }

        public void Start()
        {
            if (Running) throw new Exception("already running");
            Running = true;
            loopThread = new Thread(loop);
            loopThread.Start();
        }

        private object _ptr;//prevent garbage collection of delegate
        protected abstract object init();

        private void loop()
        {
            ThreadID = Kernel32.GetCurrentThreadId();
            _ptr=init();

            while (true)
            {
                MSG msg;
                if (!User32.GetMessage(out msg, HWND.NULL, 0, 0))
                    break;

                User32.TranslateMessage(msg);
                User32.DispatchMessage(msg);
            }

            cleanup();
        }

        protected abstract void cleanup();


        //once signaled, the event object instance shall not be accessed again by the hooker
        protected void signal(T evt)
        {
            //TODO
            if (Kernel32.GetCurrentThreadId() != ThreadID)
                throw new Exception("Cross thread call to signal");

            OnEvent?.Invoke(this, evt);

        }


        public virtual void Stop()
        {
            if (!Running) throw new Exception("already stopped");
            Running = false;
            User32.PostThreadMessage(ThreadID, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            loopThread.Join();
        }


        ~HookBase()
        {
            if (Running)
                Stop();
        }
    }

    /// <summary>
    /// Provides a base implementation for all hooks using SetWindowsHookEx
    /// </summary>
    /// <typeparam name="T">Type of event returned by the hook</typeparam>
    abstract class WindowsHookEx<T> : HookBase<T> 
    {

        private User32.SafeHHOOK hookID;
        static Kernel32.SafeHINSTANCE mar = Kernel32.LoadLibrary("user32.dll");
        private User32.HookType HookType;

        protected WindowsHookEx(User32.HookType hooktype)
        {
            HookType = hooktype;
        }


        private IntPtr HookProc1(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0)
            {
                HookProc(wParam, lParam);
            }

            return User32.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        protected override object init()
        {
            User32.HookProc del = HookProc1;
            hookID = User32.SetWindowsHookEx(
                    HookType,
                    del,
                    mar,
                    0);
            if (hookID.IsInvalid)
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();

                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }
            return del;
        }
        protected abstract void HookProc(IntPtr wParam, IntPtr lParam);

        protected override void cleanup()
        {
            if (!User32.UnhookWindowsHookEx(hookID))
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();
                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }
        }


    }

    /// <summary>
    /// Implements boilerplate code for all SetWindowsHookEx
    /// </summary>
    /// <typeparam name="T">Type of events returned</typeparam>
    abstract class BasicWindowsHookEx<T>:WindowsHookEx<T>
    {
        protected BasicWindowsHookEx(User32.HookType hooktype) : base(hooktype)
        {

        }

        protected override void HookProc(IntPtr wParam, IntPtr lParam)
        {
            signal(Marshal.PtrToStructure<T>(lParam));
        }
    }
}

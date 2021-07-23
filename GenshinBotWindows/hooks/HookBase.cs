using genshinbot.reactive.wire;
using genshinbot.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace genshinbot.hooks
{
    public class WindowsMessageLoop
    {
        public uint ThreadID { get; private set; }
        int refCnt = 0;
        Thread? t;
        private const int WM_QUIT = 0x12;
        private const int derp = 0x666;
        Queue<(Action a, TaskCompletionSource? s)> q = new();
        public IDisposable Use()
        {
            if (Interlocked.Increment(ref refCnt) == 1)
            {
                Debug.Assert(t == null);
                t = new Thread(run);
                t.Start();
            }
            return DisposableUtil.From(() =>
            {
                if (Interlocked.Decrement(ref refCnt) == 0)
                {
                    User32.PostThreadMessage(ThreadID, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                    t = null;
                }
            });
        }
        void run()
        {
            ThreadID = Kernel32.GetCurrentThreadId();
            while (Thread.VolatileRead(ref refCnt) > 0 || q.Count > 0)
            {
                MSG msg;
                if (!User32.GetMessage(out msg, HWND.NULL, 0, 0))
                    break;

                if (msg.message == derp)
                {
                    lock (q)
                        while (q.Count > 0)
                        {
                            var v = q.Dequeue();
                            v.a();
                            v.s?.SetResult();
                        }
                    continue;
                }

                User32.TranslateMessage(msg);
                User32.DispatchMessage(msg);
            }
        }
        public void RunLater(Action a)
        {
            lock (q)
            {
                q.Enqueue((a, null));
            }
            User32.PostThreadMessage(ThreadID, derp, IntPtr.Zero, IntPtr.Zero);

        }
        public static readonly WindowsMessageLoop Instance = new WindowsMessageLoop();
    }
    /// <summary>
    /// Provides a base implementation with a message loop for all hooking classes
    /// </summary>
    /// <typeparam name="T">The event type returned by the hook</typeparam>
    public abstract class HookBase<T>
    {



        /// <summary>
        /// Note: this event will be raised on the message loop thread
        /// </summary>
        public event Action<T> OnEvent;

        public bool Running { get; private set; }
        IDisposable? msgRef;
        public void Start()
        {
            if (Running) throw new Exception("already running");
            msgRef = WindowsMessageLoop.Instance.Use();
            WindowsMessageLoop.Instance.RunLater(() => _ptr = init());
        }

        private object _ptr;
        protected abstract object init();


        protected abstract void cleanup();


        //once signaled, the event object instance shall not be accessed again by the hooker
        protected void signal(T evt)
        {

            OnEvent?.Invoke(evt);

        }


        public virtual void Stop()
        {
            if (!Running) throw new Exception("already stopped");
            Running = false;
            WindowsMessageLoop.Instance.RunLater(cleanup);
            msgRef!.Dispose();
            msgRef = null;
            _ptr = null;
        }

        public IWire<T> Wire { get; }

        public HookBase()
        {
            Wire = new Wire<T>(listener =>
            {
                Action<T> bad = e => listener(e);

                OnEvent += bad;
                return DisposableUtil.From(
                    () =>
                    {
                        OnEvent -= bad;
                    }
                    );

            });
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
    public abstract class WindowsHookEx<T> : HookBase<T>
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
    public abstract class BasicWindowsHookEx<T> : WindowsHookEx<(IntPtr wParam, T lParam)>
    {
        protected BasicWindowsHookEx(User32.HookType hooktype) : base(hooktype)
        {

        }

        protected override void HookProc(IntPtr wParam, IntPtr lParam)
        {
            signal((wParam, Marshal.PtrToStructure<T>(lParam)));
        }
    }


}

using HookLib;
using HookLib.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace GenshinbotCsharp
{
    class Hooker
    {

        public HookManager hooker;
        WindowsMessagePump messagePump;
        private Thread messageThread;
        public event GlobalKeyEventHandler KeyDown;
        public event GlobalKeyEventHandler KeyUp;

        public struct MouseEventArgs
        {
            int x, y;
            int dx, dy;
        }

        public struct KeyboardEventArgs
        {
            VirtualKeyCode keyCode;
        }

        public void EnableRecordMode()
        {
            if (messageThread != null) throw new Exception("tried to start when recording");
            if (hooker == null)
            {
                hooker = new HookManager();
                messagePump = new WindowsMessagePump();
            }


            messageThread = new Thread(() => {
                hooker.KeyDown += KeyDown;
                hooker.KeyUp += KeyUp;
                messagePump.Run();
                hooker.KeyDown -= KeyDown;
                hooker.KeyUp -= KeyUp;

            });
            messageThread.Start();
        }

        public void StopRecordMode()
        {
            if (messageThread == null) throw new Exception("tried to stop when not recording");
            messagePump.Stop();
            messageThread.Join();
            messageThread = null;
        }

    }
}

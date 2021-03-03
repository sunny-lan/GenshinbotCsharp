using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenshinbotCsharp.input
{
    class MouseMover
    {
        private IInputSimulator i;
        private Point2d dst;

        private Task t;

        public bool Running { get; private set; }

        /// <summary>
        /// time between each mouse movements in millis
        /// </summary>
        public int MoveInterval=10;

        /// <summary>
        /// Max distance moved per millisecond
        /// </summary>
        public double MaxSpeed = 1;

        public MouseMover(IInputSimulator i)
        {
            this.i = i;
            dst = i.MousePos();
        }

        public Task Goto(Point2d dst)
        {
            this.dst = dst;
            if (!Running)
            {
                Running = true;
                t = Task.Run(thread);
            }
            return t;
        }

        private void thread()
        {
            while (Running)
            {
                var cur = i.MousePos();
                if (cur == dst) break;

                var delta = dst - cur;
                i.MouseMove(delta.LimitDistance(MaxSpeed * MoveInterval));
            }
            Running = false;
        }

        public void Stop()
        {
            if (!Running) return;
            Running = false;
            t.Wait();
        }

        ~MouseMover()
        {
            if (Running) Stop();
        }
    }
}

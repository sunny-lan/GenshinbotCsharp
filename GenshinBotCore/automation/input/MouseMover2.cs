using genshinbot.automation.input;
using OpenCvSharp;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.input
{
    public class MouseMover2
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

        private bool Absolute = false;

        public MouseMover2(IInputSimulator i)
        {
            this.i = i;
        }

        Point2d speed;
        /// <summary>
        /// Consistently moves mouse at a given speed
        /// </summary>
        /// <param name="speed">In pixels per millisecond</param>
        /// <returns></returns>
        public Task Move(Point2d speed) 
        {
            Absolute = false;
            this.speed = (speed*MoveInterval).LimitDistance(MaxSpeed*MoveInterval);
            if (!Running)
            {
                Running = true;
                t = Task.Run(thread);
            }
            return t;
        }

        public Task Goto(Point2d dst)
        {
            Absolute = true;
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
                if (Absolute)
                {
                    var cur = i.MousePos();
                    if (cur.DistanceTo(dst) <= MaxSpeed * MoveInterval)
                    {
                        i.MouseTo(dst);
                        break;
                    }
                    var delta = dst - cur;
                    i.MouseMove(delta.LimitDistance(MaxSpeed * MoveInterval));
                }
                else
                {
                    i.MouseMove(speed);
                }
                Thread.Sleep(MoveInterval);
            }
            Running = false;
        }

        public void Stop()
        {
            if (!Running) return;
            Running = false;
            t.Wait();
        }

        ~MouseMover2()
        {
            if (Running) Stop();
        }
    }
}

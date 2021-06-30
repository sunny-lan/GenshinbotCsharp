using genshinbot.reactive.wire;
using genshinbot.util;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.automation.input
{
    /// <summary>
    /// Meant for use fixed mouse situations
    /// Ignores the old value if still running
    /// </summary>
    class MouseSmoother
    {
        /// <summary>
        /// time between each mouse movements in millis
        /// </summary>
        public int MoveInterval = 5;

        /// <summary>
        /// Max pixel/millisecond
        /// </summary>
        public double MaxSpeed = 0.5;

        /// <summary>
        /// Max distance jumped by mouse
        /// </summary>
        public double MaxDist = 50;

        public double MaxAccel = 1;
        public IWire<Point2d> Output { get; }
        WireSource<Point2d> output = new();
        private readonly IWire<Point2d> deltas;

        public MouseSmoother(IWire<Point2d> deltas)
        {
            Output = output.OnSubscribe(() =>
            {
                CancellationTokenSource cs = new();
                var t = new Thread(() => loop(cs.Token));
                t.Start();
                return DisposableUtil.From(() =>
                {
                    cs.Cancel();
                    t.Join();

                });
            });
            this.deltas = deltas;
        }


        private void loop(CancellationToken ct)
        {
            Point2d delta = Util.Origin;
            DateTime last = DateTime.Now;
            var lastVel = 0d;
            //double dist = 0;
            using (deltas.Subscribe(x =>
            {
                delta += x;
                //dist = 0;
            }))
                while (!ct.IsCancellationRequested)
                {
                    if (delta!=Util.Origin)
                    {
                        var dd = delta;
                        var now = DateTime.Now;
                        var deltaT = (now - last).TotalMilliseconds;
                        last = now;
                        var movement = dd*deltaT;
                        movement = movement.LimitDistance(Math.Min(MaxDist,MaxSpeed*deltaT), out var ddd);
                        delta -= movement;
                        output.Emit(movement);
                        lastVel = ddd / deltaT;
                        
                        if (ddd<=0.001)
                        {
                            delta = Util.Origin;

                        }else

                        {
                            var v = 2 * MoveInterval - deltaT;
                            if(v>0)
                                Thread.Sleep((int)v);
                        }
                    }
                    else
                    {
                        deltas.Get().Wait(ct);
                    }
                }
        }
    }
}

using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Max distance moved per millisecond
        /// </summary>
        public double MaxSpeed = 1;

        public IWire<Point2d> Output { get; }
        public MouseSmoother(IWire<Point2d> deltas)
        {
            Point2d delta=Util.Origin;
            DateTime? lastTime=null;
            Output = Wire.Interval(MoveInterval)
                .Select(_ =>
                {
                    if (lastTime is null)
                        return Util.Origin;
                    var now = DateTime.Now;
                    var movement = delta.LimitDistance(
                        MaxSpeed * (now-lastTime.Expect()).TotalMilliseconds);
                    delta -= movement;
                    lastTime = now;
                    return movement;
                })
                .DependsOn(deltas.Do(x=> { 
                    delta = x;
                    lastTime = DateTime.Now;
                }).As<Point2d,object>());
        }
    }
}

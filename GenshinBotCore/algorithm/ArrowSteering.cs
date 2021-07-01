using genshinbot.reactive;
using genshinbot.reactive.wire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    /// <summary>
    /// control algorithm for controlling mouse 
    /// </summary>
    public class ArrowSteering
    {
        ///measured in pixels/radian
        private double scale = 200, maxPx=70;
        TimeSpan recharch = TimeSpan.FromSeconds(5);

        public IWire<double> MouseDelta { get; init; }
        public ArrowSteering(IWire<Pkt<double>> known, ILiveWire<double?> wanted1)
        {
            double limiter = 1;//trying to avoid oscilations
            var lastRecharge = DateTime.Now;
            bool? lastSign = null;

            MouseDelta = known.Select((Pkt<double> known) =>
            {
                double? wanted = wanted1.Value;
                double amt = (known.CaptureTime - lastRecharge) / recharch;
                lastRecharge = known.CaptureTime;
                limiter = Math.Clamp(limiter + amt, 0, 1);

                var rel = wanted is null ? 0 : known.Value.RelativeAngle(wanted.Expect());
                //detected oscilation = increase limiter
                var sign = rel > 0;
                if (sign != lastSign)
                {
                    lastSign = sign;
                    limiter /= 2;
                }
                  Console.WriteLine($"limiter={limiter} sign={sign}");


                return Math.Clamp(rel*  scale * limiter, -maxPx,maxPx);
            }) .Where(x=>x!=0).DependsOn(wanted1.As<double?,object>());
        }
    }
}

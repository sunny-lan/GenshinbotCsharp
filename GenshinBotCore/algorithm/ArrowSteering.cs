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
        private double scale = 100, maxDelta = 100;
        TimeSpan recharch = TimeSpan.FromSeconds(1);

        public IWire<double> MouseDelta { get; init; }
        public ArrowSteering(IWire<Pkt<double>> known, IWire<double?> wanted)
        {
            double limiter = 1;//trying to avoid oscilations
            var lastRecharge = DateTime.Now;
            bool? lastSign = null;

            //TODO disconnect if wanted is null
            MouseDelta = Wire.CombineLatest(wanted, known,(double? wanted,Pkt<double> known) =>
            {
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
                //  Console.WriteLine($"limiter={limiter} sign={sign}");


                return Math.Min(maxDelta, rel * scale * limiter);
            }) ;
        }
    }
}

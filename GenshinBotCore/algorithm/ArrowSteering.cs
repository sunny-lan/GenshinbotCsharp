using genshinbot.reactive;
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
        private double scale=100,maxDelta=100;
        TimeSpan recharch = TimeSpan.FromSeconds(1);

        public IObservable<double> MouseDelta { get; init; }
        public ArrowSteering(IObservable<Pkt<double>> known, IObservable<double> wanted)
        {
            //TODO more advanced shit
            
            double limiter = 1;//trying to avoid oscilations
            var lastRecharge = DateTime.Now;
            bool? lastSign = null ;

            MouseDelta = wanted.Select(wanted => known.Select(known =>
            {
                double amt = (known.CaptureTime - lastRecharge) / recharch;
                limiter = Math.Min(1, limiter + amt);

                var sign = known.Value > 0;
                if (sign != lastSign)
                {
                    lastSign = sign;
                    limiter /= 2;
                }

                return Math.Min(maxDelta, known.Value.RelativeAngle(wanted) * scale*limiter);
            })).Switch();
        }
    }
}

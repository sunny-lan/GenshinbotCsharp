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
        private double scale=2,maxDelta=50;

        public IObservable<double> MouseDelta { get; init; }
        public ArrowSteering(IObservable<Pkt<double>> known, IObservable<double> wanted)
        {
            //TODO more advanced shit
            MouseDelta = wanted.Select(wanted => known.Select(known =>
            {
                return Math.Min(maxDelta, known.Value.RelativeAngle(wanted) * scale);
            })).Switch();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.reactive.wire
{
    static partial class Wire
    {
        /// <summary>
        /// Emits specific signals on rising and falling "edges" (aka when the signal changes)
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <param name="signal"></param>
        /// <param name="rising"></param>
        /// <param name="falling"></param>
        /// <returns></returns>
        public static IWire<Out> Edge<Out>(this IWire<bool> signal, Out? rising=default, Out? falling=default)
            //where Out:struct
        {
            bool? last = null;
            return signal.Link<bool,Out>((v,next) =>
            {
                if (last != v)
                {
                    if (v) { 
                        if (rising is Out o) next(o); }
                    else { 
                        if (falling is Out o) next(o); }
                    last = v;
                }
            });
        }
    }
}

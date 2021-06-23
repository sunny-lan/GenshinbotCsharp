using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace genshinbot.reactive.wire
{
    public class LiveWire<T> : ILiveWire<T>
    {
        private T last;
        private bool running;
        private Func<T> getVal;
        Wire<T> wire;
        public bool ChecksDistinct { get;  }
        public LiveWire(Func<T> getVal, Func<Action, IDisposable> enable, bool checkDistinct=false) 
            
        {
            this.ChecksDistinct = checkDistinct;
            this.getVal = getVal;
            this.wire=new Wire<T>(onNext =>
            {
                running = true;
                
                [DebuggerHidden]
                void onParentChange()
                {
                    var next = getVal();
                    if (!checkDistinct || !EqualityComparer<T>.Default.Equals(last, next))
                    {
                        last = next;
                        onNext(next);
                    }
                }
                var sub = enable(onParentChange);
                //make sure value is uptodate
                last = getVal();
                return DisposableUtil.Merge(DisposableUtil.From( ()=> {
                    running = false;
                }), sub);
            });
        }
        public IDisposable Subscribe(Action<T> onValue)
        {
            return wire.Subscribe(onValue);
        }


        //If its running, then just use the last value
        //Else need to call getVal()
        public T Value => running ? last : getVal();
    }
}

using genshinbot.util;
using System;
using System.Collections.Generic;

namespace genshinbot.reactive.wire
{
    public class LiveWire<T> : ILiveWire<T>
    {
        private T last;
        private bool running;
        private bool upToDate = false;
        private Func<T> getVal;
        Wire<T> wire;
        public LiveWire(Func<T> getVal, Func<Action, IDisposable> enable) 
            
        {
            this.getVal = getVal;
            this.wire=new Wire<T>(onNext =>
            {
                running = true;
                

                void onParentChange()
                {
                    var next = getVal();
                    if (!EqualityComparer<T>.Default.Equals(last, next))
                    {
                        last = next;
                        onNext(next);
                    }
                }
                var sub = enable(onParentChange);
                //make sure value is uptodate
                onParentChange();
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

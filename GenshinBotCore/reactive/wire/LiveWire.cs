using genshinbot.diag;
using genshinbot.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace genshinbot.reactive.wire
{
    public class LiveWire<T> : ILiveWire<T>
    {

        private T? last;
        private object val_lock = new object();

        private bool running = false;


        private Func<T> getVal;
        Wire<T> wire;
        int
            iid = ID.get();
        public bool ChecksDistinct { get; }
        public LiveWire(Func<T> getVal, Func<Action, IDisposable> enable, bool checkDistinct = false)

        {
            this.ChecksDistinct = checkDistinct;
            this.getVal = getVal;

            this.wire = new Wire<T>(onNext =>
            {

                    void onParentChange()
                    {
                        Debug.Assert(running);
                          var next = getVal();

                            if (checkDistinct)
                            {
                                if (EqualityComparer<T>.Default.Equals(last, next)) return;
                            }
                            last = next;
                            onNext(next);

                    }

                    //make sure value is uptodate
                    last = getVal();
                    running = true;
                   // Console.WriteLine($"live {iid} running");
                    var sub = enable(onParentChange);
                    return DisposableUtil.From(() =>
                    {
                            sub.Dispose();
                           // Console.WriteLine($"live {iid} stop");
                            running = false;
                            last = default;
                    });
            });
        }
        public IDisposable Subscribe(Action<T> onValue)
        {
            return wire.Subscribe(onValue);
        }


        //If its running, then just use the last value
        //Else need to call getVal()
        public T Value
        {
            get
            {
                return running ? last : getVal();
            }
        }
    }
}

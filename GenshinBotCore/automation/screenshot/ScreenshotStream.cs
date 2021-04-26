using genshinbot.reactive;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace genshinbot.automation.screenshot
{
    class ScreenshotStream
    {
        public Mat Buf;

        private Dictionary<Rect, HashSet<Action<Mat>>> subs = new Dictionary<Rect, HashSet<Action<Mat>>>();



        public void Invalidate(Rect r)
        {
            Debug.Assert(Buf != null);
            if (!subs.TryGetValue(r, out var list)) return;
            foreach (var sub in list)
                sub(Buf[r]);
        }

        private class _Subscription : Subscription
        {
            ScreenshotStream parent;

            public _Subscription(ScreenshotStream parent, Rect r, Action<Mat> callback)
            {
                this.parent = parent;
                this.r = r;
                this.callback = callback;
            }

            Rect r;
            Action<Mat> callback;

            public void Dispose()
            {
                Debug.Assert(!disposed, "Subscriber disposed twice");
                disposed = true;
                Debug.Assert(parent.subs[r].Remove(callback));
            }
            private bool disposed = false;


            ~_Subscription()
            {
                if (!disposed)
                    Dispose();
            }
        }



        public Subscription Listen(Rect r, Action<Mat> callback)
        {
            var v = subs.GetValueOrDefault(r, new HashSet<Action<Mat>>());
            v.Add(callback);
            subs[r] = v;
            return new _Subscription(this, r, callback);
        }
    }
}

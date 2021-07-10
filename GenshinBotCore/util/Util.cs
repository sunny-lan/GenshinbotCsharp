using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot
{
    public static partial class Util
    {
       
        /// <summary>
        /// Returns an observable which publishes unit.default everytime a new element is recieved.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static IObservable<Unit> ToUnit<T>(this IObservable<T> o)
        {
            return o.Select(x => Unit.Default);
        }

        /// <summary>
        /// Merge Error and Completion notifications from another observable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="completion"></param>
        /// <returns></returns>
        public static IObservable<T> MergeNotification<T>(this IObservable<T> first, IObservable<Unit> completion)
        {
            return Observable.Merge(
                first.Materialize(),
                completion.Materialize().Select(notif =>
                {
                    switch (notif.Kind)
                    {
                        case NotificationKind.OnCompleted:
                            return Notification.CreateOnCompleted<T>();
                        case NotificationKind.OnError:
                            return Notification.CreateOnError<T>(notif.Exception);
                        default:
                            throw new Exception("Invalid notif type");
                    }
                })
            ).Dematerialize();
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        [System.Diagnostics.DebuggerHidden]
        public static T Expect<T>(this T? t, string assert = "") where T : struct
        {
            if (t == null) Debug.Fail(assert);
            return (T)t;
        }
        private static Random rng = new Random(1);

   
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        public static List<T> ToList<T>(this IEnumerator<T> enumerator)
        {
            return enumerator.ToEnumerable().ToList();
        }

        public static int LowerBound<T>(this List<T> list, Func<T, bool> pred)
        {
            int lo = 0, hi = list.Count;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (pred(list[mid]))
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;
                }
            }
            return lo;
        }



    }
}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot
{
    public static partial class Util
    {
        public static byte[] Serialize<T>(this T v)where T:struct
        {
            // Get the byte size of a MyDataStruct structure if it is to be
            // marshaled to unmanaged memory.
            Int32 iSizeOMyDataStruct = Marshal.SizeOf<T>();
            // Allocate a byte array to contain the bytes of the unmanaged version
            // of the MyDataStruct structure.
            byte[] byteArrayMyDataStruct = new byte[iSizeOMyDataStruct];
            // Allocate a GCHandle to pin the byteArrayMyDataStruct array
            // in memory in order to obtain its pointer.
            GCHandle gch = GCHandle.Alloc(byteArrayMyDataStruct, GCHandleType.Pinned);
            // Obtain a pointer to the byteArrayMyDataStruct array in memory.
            IntPtr pbyteArrayMyDataStruct = gch.AddrOfPinnedObject();
            // Copy all bytes from the managed MyDataStruct structure into
            // the byte array.
            Marshal.StructureToPtr(v, pbyteArrayMyDataStruct, false);
            // Unpin the byteArrayMyDataStruct array in memory.
            gch.Free();
            // Return the byte array.
            // It contains the serialized bytes of the MyDataStruct structure.
            return byteArrayMyDataStruct;
        }

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

        [System.Diagnostics.DebuggerHidden]
        public static T Expect<T>(this T? t, string assert = "") where T : class
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

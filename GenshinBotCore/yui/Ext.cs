using genshinbot.util;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public static partial class Ext
    {
        public static Func<K, bool> ConditionalSignal<K>(this EventWaiter<K> waiter, Func<K, bool> condition = null) where K : struct

        {
            return (e) =>
           {
               if (condition?.Invoke(e) == true)
               {
                   waiter.Signal(e);
                   return true;
               }
               else
               {
                   return false;
               }
           };
        }


        public static async Task<OpenCvSharp.Rect> SelectRect(this Viewport v)
        {
            var r = await v.SelectCreateRect();
            v.Delete(r);
            return r.R;
        }


        public static Task<Rect> SelectCreateRect(this Viewport v)
        {
            var tsk = new TaskCompletionSource<Rect>();
            var old = v.OnMouseEvent;

            bool down = false;
            Rect r = null;
            Point initial = default, final = default;
            v.OnMouseEvent = evt =>
            {
                if (!down)
                {
                    if (evt.Type == MouseEvent.Kind.Down)
                    {
                        down = true;

                        initial = evt.Location.Round();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (evt.Type == MouseEvent.Kind.Move)
                    {
                        if (r == null)
                            r = v.CreateRect();

                        final = evt.Location.Round();
                        r.R = Util.RectAround(initial, final);
                        return true;
                    }
                    else if (evt.Type == MouseEvent.Kind.Up)
                    {
                        v.OnMouseEvent = old;
                        if (r == null)
                            r = v.CreateRect();

                        final = evt.Location.Round();
                        r.R = Util.RectAround(initial, final);

                        tsk.SetResult(r);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            };




            return tsk.Task;
        }
    }
}

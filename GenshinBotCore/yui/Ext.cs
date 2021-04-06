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

        public static yui.Slider[] CreateSliders(this yui.Container o, string prefix, Scalar? v, Action<Scalar> onChange)
        {
            var expander = o.CreateExpander();
            expander.Label = prefix;
            var sidebar = expander.Content;
            var sliders = new yui.Slider[3];
            void onVChange(int _)
            {
                onChange(new Scalar(sliders[0].V, sliders[1].V, sliders[2].V));
            }
            yui.Slider createSlider(string suffix, double? v)
            {
                var slider = sidebar.CreateSlider();
                slider.Label =  suffix;
                slider.Max = 255;
                slider.Min = 0;
                if (v is double _v) slider.V = (int)_v;
                slider.VChanged += onVChange;
                return slider;
            }
            sliders[0] = createSlider("H", v?.Val0);
            sliders[1] = createSlider("S", v?.Val1);
            sliders[2] = createSlider("V", v?.Val2);

            return sliders;
        }

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


            bool down = false;
            Rect r = null;
            Point initial = default, final = default;
            void handleMouseEvent(MouseEvent evt)
            {
                if (!down)
                {
                    if (evt.Type == MouseEvent.Kind.Down)
                    {
                        down = true;

                        initial = evt.Location.Round();
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
                    }
                    else if (evt.Type == MouseEvent.Kind.Up)
                    {
                        v.MouseEvent -= handleMouseEvent;

                        if (r == null)
                            r = v.CreateRect();

                        final = evt.Location.Round();
                        r.R = Util.RectAround(initial, final);

                        tsk.SetResult(r);
                    }
                }
            };
            v.MouseEvent += handleMouseEvent;



            return tsk.Task;
        }
    }
}

using GenshinbotCsharp.data;
using GenshinbotCsharp.util;
using GenshinbotCsharp.yui;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{

    /// <summary>
    /// Platform independent abstraction of gui
    /// </summary>
    interface YUI
    {
        Tab CreateTab();


    }
}

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace GenshinbotCsharp.yui
{
    public interface Notification
    {
        string Message { get; set; }
    }

    public interface Notifications
    {
        Notification CreateNotification();
        void Delete(Notification n);
    }

    public interface Rect
    {
        bool Editable { get; set; }
        OpenCvSharp.Rect R { get; set; }
    }

    public interface Image
    {
        Mat Mat { get; set; }
        Point TopLeft { get; set; }

        /// <summary>
        /// Repaint the image without needing to replace the mat
        /// </summary>
        void Invalidate();
        void Invalidate(OpenCvSharp.Rect r);
    }

    public struct MouseEvent
    {
        public enum Kind
        {
            Up, Down, Move, Click
        }

        public Point2d Location;
        public Kind Type;
    }
    public interface Viewport
    {
        Size Size { get; set; }

        /// <summary>
        /// transformation
        /// </summary>
        Transformation T { get; set; }
        Action<Transformation> OnTChange { get; set; }

        Rect CreateRect();
        Image CreateImage();

        /// <summary>
        /// Callback should return true if event handled
        /// If false is returned event is bubbled to parent
        /// </summary>
        Func<MouseEvent, bool> OnMouseEvent { get; set; }

    }

    static class Ext
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



        public static Task<Rect> SelectAndCreate(this Viewport v)
        {
            var tsk = new TaskCompletionSource<Rect>();
            var old = v.OnMouseEvent;

            bool down = false;
            Rect r=null;
            Point initial=default, final=default;
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
                        if(r==null)
                            r = v.CreateRect();

                        final = evt.Location.Round();
                        r.R = Util.RectAround(initial, final);
                        return true;
                    }
                    else if(evt.Type==MouseEvent.Kind.Up)
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

    public interface Button
    {
        event EventHandler Click;
        string Text { get; set; }
    }

    public interface Container
    {
        Viewport CreateViewport();
        Button CreateButton();
    }

    public interface Tab
    {
        string Title { get; set; }
        Container Content { get; }

        Notifications Notifications { get; }
    }
}

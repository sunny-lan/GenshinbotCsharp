using GenshinbotCsharp.data;
using GenshinbotCsharp.yui;
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace GenshinbotCsharp
{

    /// <summary>
    /// Platform independent abstraction of gui
    /// </summary>
    public interface YUI
    {
        Tab CreateTab();

        void RemoveTab(Tab tab);
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
    public interface DirectGfx
    {
        void Rect(Rect r);
        void Image(Mat m);
    }

    public interface Viewport2
    {
        event EventHandler<DirectGfx> Paint;
        void Invalidate(Rect region);
        void Invalidate();
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

        void ClearChildren();
        void Delete(object r);

        
    }

    public interface Button
    {
        event EventHandler Click;
        string Text { get; set; }
    }

    public interface TreeView
    {
        public interface Node
        {
            Node CreateChild();
            event EventHandler DoubleClick;
            event EventHandler Selected;
            event EventHandler Deselected;
            string Text { get; set; }
            Scalar Color { get; set; }
            void Delete(Node child);
            void ClearChildren();
            void Invalidate();
        }

        public Node CreateNode();
        void Delete(Node child);
        void ClearChildren();


        public void BeginUpdate();
        public void EndUpdate();
    }

    public interface PropertyGrid
    {
        public object SelectedObject { get; set; }
    }

    public interface Container
    {
        Viewport CreateViewport();
        Button CreateButton();

        TreeView CreateTreeview();

        PropertyGrid CreatePropertyGrid();
        Container CreateSubContainer();

        void ClearChildren();
        void Delete(object btn);

        Viewport2 GetViewport2() { throw new NotImplementedException(); }
    }

    public interface Tab
    {
        string Title { get; set; }
        Container Content { get; }

        public string Status { get; set; }
        Notifications Notifications { get; }
    }
}

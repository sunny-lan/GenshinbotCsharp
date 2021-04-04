using genshinbot.data;
using genshinbot.yui;
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace genshinbot
{

    /// <summary>
    /// Platform independent abstraction of gui
    /// </summary>
    public interface YUI
    {
        Tab CreateTab();

        void RemoveTab(Tab tab);

        /// <summary>
        /// Return true to cancel the close event
        /// </summary>
        Func<bool> OnClose { get; set; }

        /// <summary>
        /// Show a message box. Can only be closed by user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void Popup(string message, string title = "");
        void GiveFocus(Tab t);
    }
}

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
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

    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public interface Line
    {
        Point A { get; set; }
        Point B { get; set; }
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
        Line CreateLine();
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

        bool Enabled { get; set; }
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

        public void GiveFocus(Node n);
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
        void Delete(object child);

        Viewport2 GetViewport2() { throw new NotImplementedException(); }

        /// <summary>
        /// Used to add a unsupported child to the container
        /// For example directly adding a Windows Forms control to the container.
        /// </summary>
        /// <param name="unknown"></param>
        void AddExternal(object unknown) { throw new NotImplementedException(); }
    }

    public interface Tab
    {
        string Title { get; set; }
        Container Content { get; }

        public string Status { get; set; }
       // Notifications Notifications { get; }
    }
}

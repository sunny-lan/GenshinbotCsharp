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
        PopupResult Popup(string message, string title = "", PopupType type=PopupType.Message);
        void GiveFocus(Tab t);
    }
}

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public enum PopupType
    {
        Message, Confirm,
    }
    public enum PopupResult
    {
        Ok,Cancel
    }
    public interface Deletable
    {
        void Delete();
    }
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Flexbox model:
    ///  - Items cannot determine their own size
    ///  - They can only provide a minimum valid size
    /// Items are expanded to fit along the secondary axis,
    /// and fitted based on weight on the primary axis
    /// 
    /// The minimum valid size of the flexbox itself is equal to the minimum size required to fit all the items
    ///  - If wrap is enabled, the minimum size if scaled along the primary axis is the maximum width of a single item
    ///  - If scroll is enabled 
    /// </summary>
    public class Flexbox
    {
        public class Item
        {
            /// <summary>
            /// 0 = AutoSize to contents
            /// >1 = Fill up rest of space according to weight
            /// </summary>
            public int Weight;
        }

        /// <summary>
        /// Whether items are listed from left to right or from top to bottom
        /// </summary>
        public Orientation Direction = Orientation.Horizontal;

        /// <summary>
        /// Whether to wrap when not enough space
        /// </summary>
        public bool Wrap = false;

        /// <summary>
        /// Whether to show scroll upon overflow
        /// If both scroll and wrap are enabled, wrap will happen first, and then scroll
        /// The scroll will happen in the direction of the wrap
        ///     For example, if items are placed left to right, the rows will not get longer,
        ///     It will only scroll to show more rows
        /// </summary>
        public bool Scroll = false;
    }
    public interface Expander
    {
        public bool Expanded { get; set; }
        string Label { get; set; }
        Container Content { get; }
    }
    public interface Container
    {
        Label CreateLabel();
        Expander CreateExpander();
        Viewport CreateViewport();
        Button CreateButton();

        TreeView CreateTreeview();

        PropertyGrid CreatePropertyGrid();
        Container CreateSubContainer();

        Slider CreateSlider();

        void ClearChildren();
        void Delete(object child);

        bool SupportsFlexbox => false;

        /// <summary>
        /// Should simply ignore unsupported features
        /// Does not support dynamic layout:
        ///     - Should not be called more than once
        /// </summary>
        void SetFlex(Flexbox layout) { }
        /// <summary>
        /// Should not be called more than once per child
        /// </summary>
        /// <param name="child"></param>
        /// <param name="layout"></param>
        void SetFlex(object child, Flexbox.Item layout) { }

        void SuspendLayout();
        void ResumeLayout();

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

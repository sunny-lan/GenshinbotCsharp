
using GenshinbotCsharp.data;
using OpenCvSharp;
using System;
using System.Collections.Generic;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace GenshinbotCsharp.yui
{

    public interface Rect {
        bool Editable { get; set; }
        OpenCvSharp.Rect R { get; set; }
    }

    public interface Image
    {
        Mat image { get; set; }
        Point TopLeft { get; set; }
    }

    public interface Viewport
    {
        Size Size { get; set; }

        Transformation Transformation { get; set; }

        Rect CreateRect();
        Image CreateImage();

    }

    public interface Button
    {
        event EventHandler Click;
        string Text { get;set; }
    }

    public interface Container
    {
        Viewport CreateViewport();
        Button CreateButton();
    }

    public interface Tab 
    {
        string Title { get; set; }
        Container Content { get;}
    }

    interface YUI
    {
        Tab CreateTab();
    }
}

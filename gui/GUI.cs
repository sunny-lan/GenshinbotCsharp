
using OpenCvSharp;
using System;

namespace GenshinbotCsharp.gui
{

    interface MouseEventSource
    {
        event EventHandler<Point2d> OnMouseMove;
        event EventHandler<Point2d> OnMouseDown;
        event EventHandler<Point2d> OnMouseUp;
    }

    interface Point
    {

    }
    interface Image:MouseEventSource
    {
        Point Point();
        void SetMat(Mat m);
        void Repaint();
    } 

    interface Tab {
         Image Image(string key);
    }
    interface GUI
    {
         Tab Tab(string key);

    }
}

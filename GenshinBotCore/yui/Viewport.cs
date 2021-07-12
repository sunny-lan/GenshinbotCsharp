using genshinbot.data;
using OpenCvSharp;
using System;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public interface Clickable
    {

        event Action<MouseEvent>? MouseEvent;
    }
    public interface Colorable
    {
        Scalar Color { get; set; }
    }
    public interface Viewport:Clickable
    {
        Size Size { get; set; }

        /// <summary>
        /// transformation
        /// </summary>
        Transformation T { get; set; }
        Action<Transformation>? OnTChange { get; set; }

        Rect CreateRect();
        Line CreateLine();
        Image CreateImage();


        void ClearChildren();
        void Delete(object r);

        Point2d? MousePos { get; }
        
    }


    public interface Rect:Clickable,Colorable
    {
        OpenCvSharp.Rect R { get; set; }
    }

    public interface Image:Clickable
    {
        Mat ?Mat { get; set; }
        Point TopLeft { get; set; }

        /// <summary>
        /// Repaint the image without needing to replace the mat
        /// </summary>
        void Invalidate();
        void Invalidate(OpenCvSharp.Rect r);
    }

    public interface Line : Clickable,Colorable
    {
        Point A { get; set; }
        Point B { get; set; }
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

}

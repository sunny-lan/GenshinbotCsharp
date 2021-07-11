using genshinbot.data;
using OpenCvSharp;
using System;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
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

        event Action<MouseEvent> MouseEvent;

        void ClearChildren();
        void Delete(object r);

        Point2d? MousePos { get; }
        
    }


    public interface Rect
    {
        OpenCvSharp.Rect R { get; set; }
        event Action<MouseEvent> MouseEvent;
    }

    public interface Image
    {
        Mat ?Mat { get; set; }
        Point TopLeft { get; set; }

        /// <summary>
        /// Repaint the image without needing to replace the mat
        /// </summary>
        void Invalidate();
        void Invalidate(OpenCvSharp.Rect r);
        event Action<MouseEvent> MouseEvent;
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

    public interface Line
    {
        Point A { get; set; }
        Point B { get; set; }
    }
}

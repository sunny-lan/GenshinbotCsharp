using OpenCvSharp;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public struct MouseEvent
    {
        public enum Kind
        {
            Up, Down, Move, Click
        }

        public Point2d Location;
        public Kind Type;
    }
}

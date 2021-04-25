using OpenCvSharp;

namespace genshinbot.automation
{
    public interface Frame
    {
        Mat this[Rect subrect] { get; }
        Size Size { get; }

        Mat Mat => this[new Rect(Util.Origin, Size)];
    }
}

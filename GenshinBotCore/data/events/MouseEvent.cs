using genshinbot.automation.input;
using OpenCvSharp;

namespace genshinbot.data.events
{
    public record MouseEvent
    {
        public Point2d Position { get; init; }

    }
    public record MoveEvent : MouseEvent
    {

    }
    public record ClickEvent : MouseEvent
    {
        public MouseBtn Button { get; init; }
        public bool Down { get; init; }
    }
}

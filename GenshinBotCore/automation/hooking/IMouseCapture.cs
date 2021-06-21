using genshinbot.automation.input;
using OpenCvSharp;
using System;

namespace genshinbot.automation.hooking
{
    public interface IMouseCapture
    {
        public record MouseEvent { 
            public Point2d Position { get; init; }
            
        }
        public record MoveEvent:MouseEvent
        {

        }
        public record ClickEvent:MouseEvent
        {
            public MouseBtn Button { get; init; }
            public bool Down { get; init; }
        }

        public IObservable<MouseEvent> MouseEvents { get; }


    }
}

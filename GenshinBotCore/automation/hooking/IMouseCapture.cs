using genshinbot.automation.input;
using genshinbot.reactive.wire;
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

        public IWire<MouseEvent> MouseEvents { get; }


    }
}

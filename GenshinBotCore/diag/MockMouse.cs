using genshinbot.automation.input;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace genshinbot.diag
{
    public class MockMouse : IMouseSimulator2
    {
        public Task MouseButton(MouseBtn btn, bool down)
        {
            throw new NotImplementedException();
        }

        public Task MouseMove(Point2d d)
        {
            throw new NotImplementedException();
        }

        public Task<Point2d> MousePos()
        {
            throw new NotImplementedException();
        }

        public Task MouseTo(Point2d p)
        {
            throw new NotImplementedException();
        }
    }
}

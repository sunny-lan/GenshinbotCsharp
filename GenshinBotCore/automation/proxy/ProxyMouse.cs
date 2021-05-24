using genshinbot.automation.input;
using genshinbot.reactive;
using OpenCvSharp;
using System.Diagnostics;
using System.Threading.Tasks;

namespace genshinbot.automation
{
    public class ProxyMouse : IMouseSimulator2
    {
        IObservableValue<bool> enabled;

        IMouseSimulator2 m;

        public ProxyMouse(IObservableValue<bool> enabled, IMouseSimulator2 m)
        {
            this.enabled = enabled;
            this.m = m;
        }

        public Task MouseButton(MouseBtn btn, bool down)
        {
            Debug.Assert(enabled.Value);
            return m.MouseButton(btn, down);
        }

        public Task MouseMove(Point2d d)
        {
            Debug.Assert(enabled.Value);
            return m.MouseMove(d);
        }

        public Task<Point2d> MousePos()
        {
            Debug.Assert(enabled.Value);
            return m.MousePos();
        }

        public Task MouseTo(Point2d p)
        {
            Debug.Assert(enabled.Value);
            return m.MouseTo(p);
        }
    }
}

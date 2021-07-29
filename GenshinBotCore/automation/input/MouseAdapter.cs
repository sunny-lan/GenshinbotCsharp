using OpenCvSharp;
using System.Threading.Tasks;

namespace genshinbot.automation.input
{
    public class MouseAdapter : IMouseSimulator2
    {
        protected readonly IMouseSimulator2 wrap;

        public MouseAdapter(IMouseSimulator2 wrap)
        {
            this.wrap = wrap;
        }

        public virtual Task MouseButton(MouseBtn btn, bool down)
        {
            return wrap.MouseButton(btn, down);
        }

        public virtual Task MouseMove(Point2d d)
        {
            return wrap.MouseMove(d);
        }

        public virtual Task<Point2d> MousePos()
        {
            return wrap.MousePos();
        }

        public virtual Task MouseTo(Point2d p)
        {
            return wrap.MouseTo(p);
        }

        public virtual Task MouseClick(MouseBtn btn)
        {
            return wrap.MouseClick(btn);
        }
    }
}

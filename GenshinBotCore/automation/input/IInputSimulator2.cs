using OpenCvSharp;
using System.Threading.Tasks;

namespace genshinbot.automation.input
{
    public interface IMouseSimulator2
    {

        Task<Point2d> MousePos();
        Task MouseMove(Point2d d);
        Task MouseTo(Point2d p);

        Task MouseDown(MouseBtn btn) => MouseButton(btn, true);
        Task MouseUp(MouseBtn btn) => MouseButton(btn, false);
        async Task MouseClick(MouseBtn btn)
        {
            await MouseDown(btn);
            await MouseUp(btn);
        }

        Task MouseButton(MouseBtn btn, bool down);
    }

    public interface IKeySimulator2
    {
        //TODO change everything to use enum
        Task Key(Keys k, bool down);
        Task KeyDown(Keys k) => Key(k, true);

        Task KeyUp(Keys k) => Key(k, false);

        async Task KeyPress(Keys k)
        {
            await KeyDown(k);
            await Task.Delay(10);
            await KeyUp(k);
        }
    }
}

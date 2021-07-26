using OpenCvSharp;
using System.Threading.Tasks;

namespace genshinbot.automation.input
{
    public interface IMouseSimulator2
    {
        /// <summary>
        /// Return mouse pixel position
        /// </summary>
        /// <returns></returns>
        Task<Point2d> MousePos();

        /// <summary>
        /// Send a mouse delta pixels (regardless of position)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        Task MouseMove(Point2d d);

        /// <summary>
        /// Set mouse position to absolute pixel location
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        Task MouseTo(Point2d p);

        Task MouseDown(MouseBtn btn) => MouseButton(btn, true);
        Task MouseUp(MouseBtn btn) => MouseButton(btn, false);
        async Task MouseClick(MouseBtn btn)
        {
            await MouseDown(btn).ConfigureAwait(false);
            await MouseUp(btn).ConfigureAwait(false);
        }
        async Task MouseClick(MouseBtn btn, Point pos)
        {
            await MouseTo(pos).ConfigureAwait(false);
            await MouseClick(btn).ConfigureAwait(false);
        }
        async Task LeftClick(Point pos)
        {
            await MouseClick(MouseBtn.Left, pos).ConfigureAwait(false);
        }
        Task MouseButton(MouseBtn btn, bool down);
        Task MouseTo(int newX, int newY)
        {
            return MouseTo(new(newX, newY));
        }
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
            await KeyUp(k);
        }
    }
}

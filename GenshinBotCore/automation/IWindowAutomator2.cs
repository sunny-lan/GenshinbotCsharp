
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace genshinbot.automation
{
    /// <summary>
    /// New version interface for automation, updated to use Reactive programming
    /// </summary>
    public interface IWindowAutomator2
    {
        /// <summary>
        /// whether the window is focused
        /// </summary>
        ILiveWire<bool> Focused { get; }

        Task WaitForFocus(TimeSpan? timeout = null)
        {
            return Focused.WaitTrue(timeout);
        }

        /// <summary>
        /// The size of the window
        /// </summary>
        IWire<Size> Size { get; }

        /// <summary>
        /// The bounds of the window
        /// Recommended to override for performance
        /// </summary>
        IWire<Rect> Bounds => Size.Select(s => s.Bounds());

        /// <summary>
        /// The real position of the window on screen
        /// </summary>
        IWire<Rect> ScreenBounds { get; }

        /// <summary>
        /// Try to focus the window
        /// </summary>
        void TryFocus();

        //Point ClientToScreen(Point p);

        input.IKeySimulator2 Keys { get; }
        input.IMouseSimulator2 Mouse { get; }
        screenshot.ScreenshotObservable Screen { get; }

        hooking.IMouseCapture MouseCap { get; }
        hooking.IKeyCapture KeyCap { get; }

    }
}
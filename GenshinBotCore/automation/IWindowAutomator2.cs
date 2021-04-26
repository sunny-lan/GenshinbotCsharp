using genshinbot.reactive;
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
        IObservableValue<bool> Focused { get; }

        /// <summary>
        /// The bounds of the window. The x,y position should always be 0
        /// </summary>
        IObservableValue<Rect> Bounds { get; }

        /// <summary>
        /// Try to focus the window
        /// </summary>
        void TryFocus();

        input.IKeySimulator Keys { get; }
        input.IMouseSimulator Mouse { get; }
        screenshot.ScreenshotObservable Screen {get;}

}
}
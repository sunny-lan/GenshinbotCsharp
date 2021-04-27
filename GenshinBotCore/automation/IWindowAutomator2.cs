using genshinbot.reactive;
using OpenCvSharp;
using System;
using System.Reactive.Linq;
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
        IObservable<bool> Focused { get; }

        Task WaitForFocus(TimeSpan? timeout = null)
        {
            return Focused.WaitTrue(timeout);
        }

        /// <summary>
        /// The size of the window
        /// </summary>
        IObservable<Size> Size { get; }

        /// <summary>
        /// Try to focus the window
        /// </summary>
        void TryFocus();

        input.IKeySimulator2 Keys { get; }
        input.IMouseSimulator2 Mouse { get; }
        screenshot.ScreenshotObservable Screen { get; }

    }
}
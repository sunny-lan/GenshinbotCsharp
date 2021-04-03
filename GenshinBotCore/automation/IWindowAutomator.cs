using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    public interface IWindowAutomator : input.IInputSimulator
    {
        bool Focused { get; }

        event EventHandler<Rect> OnClientAreaChanged;
        event EventHandler<bool> OnFocusChanged;

        Rect GetBounds();
        Scalar GetPixelColor(int x, int y);
        Size GetSize();
        bool IsForegroundWindow();
        Mat Screenshot(Rect r);

        Task<Mat> ScreenshotAsync(Rect r) {
            return Task.Run(() => Screenshot(r));
        }

        void TryFocus();
        void WaitForFocus(int timeout = -1);
    }
}
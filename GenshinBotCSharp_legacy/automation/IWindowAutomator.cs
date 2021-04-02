using OpenCvSharp;
using System;

namespace GenshinbotCsharp
{
    interface IWindowAutomator: input.IInputSimulator
    {
        bool Focused { get; }

        event EventHandler<Rect> OnClientAreaChanged;
        event EventHandler<bool> OnFocusChanged;

        Rect GetBounds();
        Scalar GetPixelColor(int x, int y);
        Size GetSize();
        bool IsForegroundWindow();
        Mat Screenshot(Rect r);
        void TryFocus();
        void WaitForFocus(int timeout = -1);
    }
}
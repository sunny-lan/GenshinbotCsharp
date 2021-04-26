using OpenCvSharp;
using System;

namespace genshinbot.automation.screenshot
{
    public interface ScreenshotObservable
    {
        IObservable<Mat> Watch(Rect r);
    }
}

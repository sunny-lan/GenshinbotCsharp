using genshinbot.reactive;
using OpenCvSharp;
using System;
using System.Reactive.Linq;

namespace genshinbot.automation.screenshot
{
    public interface ScreenshotObservable
    {
        /// <summary>
        /// Watch a fixed rect
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        IObservable<Pkt<Mat>> Watch(Rect r);

        /// <summary>
        /// Watch a dynamic region
        /// </summary>
        /// <param name="r">The region to watch</param>
        /// <returns></returns>
        IObservable<Pkt<Mat>> Watch(IObservable<Rect> r)
        {
            return r.Select(rec => Watch(rec)).Switch();
        }


    }
}

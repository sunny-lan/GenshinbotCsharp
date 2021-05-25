using genshinbot.automation.screenshot;
using genshinbot.reactive;
using OpenCvSharp;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace genshinbot.diag
{
    public class MockScreenshot : ScreenshotObservable
    {
        public TimeSpan FrameInterval { get; init; }
        public Mat Image { get; set; }

        public IObservable<Pkt<Mat>> Watch(Rect r)
        {
            return Observable.Interval(FrameInterval)
                .Select(_ => Image[r])
                .Packetize();
        }
    }
}

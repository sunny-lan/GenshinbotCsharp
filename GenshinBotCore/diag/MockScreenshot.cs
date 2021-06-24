﻿using genshinbot.automation.screenshot;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace genshinbot.diag
{
    public class MockScreenshot : ScreenshotObservable
    {
        public TimeSpan FrameInterval { get; init; }
        public Func<Mat> GetImg { get; init; }

        public IWire<Pkt<Mat>> Watch(Rect r)
        {
            return Observable.Interval(FrameInterval)
                .ToWire()
                .Select(_ => GetImg()[r])
                .Packetize();
        }
    }
}

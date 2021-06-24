using genshinbot.automation;
using genshinbot.automation.hooking;
using genshinbot.automation.input;
using genshinbot.automation.screenshot;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace genshinbot.diag
{
    public class MockGenshinWindow : IWindowAutomator2
    {

        public class MockScreen
        {
            public string Name { get; init; }
            public Mat Image;
            public Dictionary<Keys, MockScreen> KeyMap { get;  } = new Dictionary<Keys, MockScreen>();
        }

        class MockKeySim : IKeySimulator2
        {
            MockGenshinWindow w;

            public MockKeySim(MockGenshinWindow w)
            {
                this.w = w;
            }

            public Task Key(Keys k, bool down)
            {
                if (down)
                    w.CurrentScreen = w.CurrentScreen.KeyMap[k];
                return Task.CompletedTask;
            }
        }

        public MockScreen PlayingScreen { get;  } = new MockScreen { Name="Playing screen"};
        public MockScreen MapScreen { get;  } = new MockScreen { Name="Map Screen"};

        public MockScreen CurrentScreen
        {
            get => currentScreen; set
            {

                currentScreen = value;

                Console.WriteLine($"Switch to {currentScreen.Name}");
                //screen.Image = currentScreen.Image;
            }
        }
        private MockScreen currentScreen;

        //For now do a consistent 20fps
        public ScreenshotObservable Screen => screen;
        private MockScreenshot screen;
        //TODO make into full observable 

        public ILiveWire<Size?> Size => size;
        private LiveWireSource<Size?> size;

        public IKeySimulator2 Keys => new MockKeySim(this);

        public IMouseSimulator2 Mouse => new MockMouse();

        public MockGenshinWindow(Size s)
        {
            screen = new MockScreenshot
            {
                FrameInterval = TimeSpan.FromSeconds(1) / 20,
                GetImg=()=> CurrentScreen.Image
            };

            size = new LiveWireSource<Size?>(s);
            PlayingScreen.KeyMap[automation.input.Keys.M] = MapScreen;
            MapScreen.KeyMap[automation.input.Keys.M] = PlayingScreen;
            MapScreen.KeyMap[automation.input.Keys.Escape] = PlayingScreen;
        }


        /// <summary>
        /// function used by external tester to manually set if the window is focussed
        /// </summary>
        /// <param name="b"></param>
        public void SetFocus(bool b)
        {
            focused.SetValue(b);
        }
        public ILiveWire<bool> Focused => focused;

        public IMouseCapture MouseCap => throw new NotImplementedException();

        public IKeyCapture KeyCap => throw new NotImplementedException();

        public ILiveWire<Rect?> ScreenBounds => throw new NotImplementedException();

        private LiveWireSource<bool> focused = new LiveWireSource<bool>(true);

        public void TryFocus()
        {
            SetFocus(true);
        }
    }
}

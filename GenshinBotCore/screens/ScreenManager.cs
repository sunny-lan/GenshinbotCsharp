using genshinbot.automation;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace genshinbot.screens
{
    public abstract class IScreen
    {
        public BotIO Io { get; }
        public ScreenManager ScreenManager { get; }

        protected IScreen(BotIO b, ScreenManager screenManager)
        {
            this.Io = b;
            this.ScreenManager = screenManager;
        }

        public virtual IWire<(bool isThis, double score)>? IsCurrentScreen(BotIO b)
        {
            return null;
        }
    }
    public class ScreenManager
    {
        public readonly BotIO io;

        public PlayingScreen PlayingScreen { get; }
        public MapScreen MapScreen { get; }
        public LoadingScreen LoadingScreen { get; }

        public ILiveWire<IScreen?> ActiveScreen => screen;
        private LiveWireSource<IScreen?> screen;

        public ScreenManager(BotIO io)
        {
            this.io = io;
            screen = new LiveWireSource<IScreen?>(null);

            PlayingScreen = new PlayingScreen(new ProxyBotIO(ActiveScreen.Select(s => s == PlayingScreen), io), this);
            MapScreen = new MapScreen(new ProxyBotIO(ActiveScreen.Select(s => s == MapScreen), io), this);
            LoadingScreen = new LoadingScreen(new ProxyBotIO(ActiveScreen.Select(s => s == LoadingScreen), io), this);

        }
        public void ForceScreen(IScreen? s)
        {
            screen.SetValue(s);
        }
        public async Task ExpectScreen(IScreen s, int timeout = 2000)
        {
            if (s.IsCurrentScreen(io) is null)
            {
                Console.WriteLine($"warn: {s.GetType().Name} IsCurrentScreen not implemented");
                ForceScreen(null);
                await Task.Delay(timeout);
            }
            else
            {
                Debug.Assert(s == await ExpectOneOf(new[] { s }, timeout));
            }
            ForceScreen(s);
        }

        public async Task<IScreen> ExpectOneOf(IScreen[] screens, int timeout = 2000)
        {
            ForceScreen(null);
            try
            {
                var res = await Wire.Merge(
                    screens.Select(screen =>
                        screen.IsCurrentScreen(io)!
                            .Where(isOpen => isOpen.isThis)
                            .Select(_ => screen)
                    )
                ).Get(TimeSpan.FromMilliseconds(timeout));
                ForceScreen(res);
                return res;
            }
            catch (TimeoutException)
            {

                ForceScreen(null);
                throw;
            }
        }
    }
}

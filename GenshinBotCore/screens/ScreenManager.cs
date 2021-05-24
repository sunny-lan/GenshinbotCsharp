using genshinbot.automation;
using genshinbot.reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.screens
{
    public abstract class IScreen
    {
        public BotIO Io { get; private init; }
        public ScreenManager ScreenManager { get; private init; }

        protected IScreen(BotIO b, ScreenManager screenManager)
        {
            this.Io = b;
            this.ScreenManager = screenManager;
        }
    }
    public class ScreenManager
    {
        private BotIO io;

        public PlayingScreen PlayingScreen { get; private init; }
        public MapScreen MapScreen { get; private init; }

        public IObservableValue<IScreen> ActiveScreen { get; private init; }
        private Subject<IScreen> screen;

        public ScreenManager(BotIO io)
        {
            this.io = io;
            screen = new Subject<IScreen>();
            ActiveScreen = screen.From(null);
            PlayingScreen = new PlayingScreen(new ProxyBotIO(ActiveScreen.Select(s => 
            s == PlayingScreen
                ), io), this);
            MapScreen = new MapScreen(new ProxyBotIO(ActiveScreen.Select(s => s == MapScreen), io), this);

        }
        public async Task ForceScreen(IScreen s)//TODO no async needed
        {
            screen.OnNext(s);
            await ActiveScreen.TakeUntil(x => x == s);
        }
        public async Task ExpectScreen(IScreen s, int timeout = 2000)
        {
            //TODO stuff
            await ForceScreen(null);
            await Task.Delay(timeout);
            await ForceScreen(s);
        }


    }
}

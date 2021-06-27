using genshinbot.automation;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.screens
{
    public abstract class IScreen
    {
        public BotIO Io { get;  }
        public ScreenManager ScreenManager { get;  }

        protected IScreen(BotIO b, ScreenManager screenManager)
        {
            this.Io = b;
            this.ScreenManager = screenManager;
        }
    }
    public class ScreenManager
    {
        private BotIO io;

        public PlayingScreen PlayingScreen { get;  }
        public MapScreen MapScreen { get;  }

        public ILiveWire<IScreen?> ActiveScreen => screen;
        private LiveWireSource<IScreen?> screen;

        public ScreenManager(BotIO io)
        {
            this.io = io;
            screen = new LiveWireSource<IScreen?>(null);
            
            PlayingScreen = new PlayingScreen(new ProxyBotIO(ActiveScreen.Select(s => 
            s == PlayingScreen
                ), io), this);
            MapScreen = new MapScreen(new ProxyBotIO(ActiveScreen.Select(s => s == MapScreen), io), this);

        }
        public void ForceScreen(IScreen? s)//TODO no async needed
        {
            screen.SetValue(s);
        }
        public async Task ExpectScreen(IScreen s, int timeout = 2000)
        {
            //TODO stuff
             ForceScreen(null);
            await Task.Delay(timeout);
             ForceScreen(s);
        }


    }
}

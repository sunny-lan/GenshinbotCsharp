using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.input
{
    enum Keys
    {
       Forward,
       Back,Left,Right,Dash,Attack,Map
    }
    class GenshinKeymap
    {
        private IInputSimulator s;
        Dictionary<Keys, int> MapKbd, MapMs;//TODO actually add keymap

        public GenshinKeymap(IInputSimulator sim)
        {
            this.s = sim;
        }

        void cvt(Keys k, Action<int> a, Action<int> b)
        {
            if (MapKbd.ContainsKey(k))
                a(MapKbd[k]);
            if (MapMs.ContainsKey(k))
                b(MapMs[k]);
            throw new ArgumentException("unmapped key");
        }

        public void KeyDown(Keys k) {
            cvt(k, s.KeyDown, s.MouseDown);
        }

        public void KeyUp(Keys k)
        {
            cvt(k, s.KeyUp, s.MouseUp);
        }

        public void KeyPress(Keys k)
        {
            cvt(k, s.KeyPress, s.MouseClick);
        }

    }
}

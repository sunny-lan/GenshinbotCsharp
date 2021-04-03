using genshinbot.automation.input;
using genshinbot.Core.automation.input;
using System;
using System.Collections.Generic;

namespace genshinbot.input
{
    public enum GenshinKeys
    {
       Forward,
       Back,Left,Right,Dash,Attack,Map
    }
    public class GenshinKeymap
    {
        private IInputSimulator s;
        Dictionary<GenshinKeys, int> MapKbd=new Dictionary<GenshinKeys, int> { 
            [GenshinKeys.Map]= (int)Keys.M,
            [GenshinKeys.Forward]= (int)Keys.W,
        };
        Dictionary<GenshinKeys, int>  MapMs=new Dictionary<GenshinKeys, int> {
            [GenshinKeys.Attack]=0,
        };

        public GenshinKeymap(IInputSimulator sim)
        {
            this.s = sim;
        }

        void cvt(GenshinKeys k, Action<int> a, Action<int> b)
        {
            if (MapKbd.ContainsKey(k))
                a(MapKbd[k]);
            else if (MapMs.ContainsKey(k))
                b(MapMs[k]);
            else
                throw new ArgumentException("unmapped key");
        }

        public void KeyDown(GenshinKeys k) {
            cvt(k, s.KeyDown, s.MouseDown);
        }

        public void KeyUp(GenshinKeys k)
        {
            cvt(k, s.KeyUp, s.MouseUp);
        }

        public void KeyPress(GenshinKeys k)
        {
            cvt(k, s.KeyPress, s.MouseClick);
        }

    }
}

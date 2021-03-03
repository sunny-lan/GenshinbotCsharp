using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using  System.Windows.Forms;

namespace GenshinbotCsharp.input
{
    enum GenshinKeys
    {
       Forward,
       Back,Left,Right,Dash,Attack,Map
    }
    class GenshinKeymap
    {
        private IInputSimulator s;
        Dictionary<GenshinKeys, int> MapKbd=new Dictionary<GenshinKeys, int> { 
            [GenshinKeys.Map]= (int)Keys.M,
        };
        Dictionary<GenshinKeys, int>  MapMs=new Dictionary<GenshinKeys, int> { };

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

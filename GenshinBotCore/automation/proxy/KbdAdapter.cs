using genshinbot.automation.input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation.proxy
{
    public class KbdAdapter:IKeySimulator2
    {
        protected readonly IKeySimulator2 wrap;

        public KbdAdapter(IKeySimulator2 wrap)
        {
            this.wrap = wrap;
        }

        public virtual Task Key(Keys k, bool down)
        {
            return wrap.Key(k, down);
        }
        public virtual Task KeyDown(Keys k) => wrap.KeyDown(k);

        public virtual Task KeyUp(Keys k) => wrap.KeyUp(k);

        public virtual Task KeyPress(Keys k) => wrap.KeyPress(k);
    }
}

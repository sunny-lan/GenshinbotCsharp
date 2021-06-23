using System.Threading;

namespace genshinbot.util
{
    public class VolatileBool
    {
        private int _value = 0;

        public bool Value
        {
            get { return Thread.VolatileRead(ref _value) == 1; }
            set
            {
                Thread.VolatileWrite(ref _value, value ? 1 : 0);
            }
        }
    }
}

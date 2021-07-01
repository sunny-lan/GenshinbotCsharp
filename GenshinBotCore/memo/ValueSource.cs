using System.Threading.Tasks;

namespace genshinbot.memo
{
    public class ValueSource<T> 
    {
        T _val;

        public ValueSource(T val)
        {
            _val = val;
        }

        TokenSource ts=new();
        public void Set(T v)
        {
            _val = v;
            ts.Update();
        }

        public Mem<T> Get()
        {
            return new Mem<T>(_val, ts.Token);
        }
    }
}

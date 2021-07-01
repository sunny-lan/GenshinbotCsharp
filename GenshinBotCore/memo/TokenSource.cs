using System.Threading;
using System.Threading.Tasks;

namespace genshinbot.memo
{
    public class TokenSource
    {
        public Task Token => tsc.Task;
        private TaskCompletionSource tsc=new TaskCompletionSource();
        public void Update()
        {
            Interlocked.Exchange(ref tsc, new TaskCompletionSource()).SetResult();
        }
    }
}

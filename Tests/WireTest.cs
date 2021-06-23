using genshinbot.reactive.wire;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GenshinBotTests
{
    public class WireTest
    {
        [Fact(Timeout =1000)]
        public void Circular()
        {
            WireSource<int> w = new WireSource<int>();
            WireSource<int> w2 = new WireSource<int>();
            var circ=w.Select(x => x == 1 ? w : w2).Switch();
            using (circ.Subscribe(x => Console.WriteLine(x)))
            {
                w.Emit(2);
                w.Emit(1);
                w.Emit(2);
            }
        }
        [Fact(Timeout = 1000)]
        public void CircularDispose()
        {
            WireSource<int> w = new WireSource<int>();
            IDisposable ?sub1=null,sub2;
            sub2 = w.Subscribe(_=>sub1!.Dispose());
             sub1 = w.Select(x=>x+2).Subscribe(_=>sub2.Dispose());
            
                w.Emit(1);
            
        }

        [Fact]
        public async Task Collision()
        {
            WireSource<int> w = new WireSource<int>();
            var sub = w.Select(x => x + 1).Where(x=>true).Select(x=>x+1);

            
            var a=Task.Run(() => { for(int i=0;i<10000;i++) w.Emit(1); });
           for(int i = 0; i < 10000; i++)
            {
                var subsp = sub.Subscribe(x => Console.WriteLine(x));
                subsp.Dispose();
            }
            await a;
        }
    }
}

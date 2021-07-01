using genshinbot.reactive;
using genshinbot.reactive.wire;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GenshinBotTests
{
    public class WireTest:MakeConsoleWork
    {
        public WireTest(ITestOutputHelper output) : base(output)
        {
        }

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

        [Fact]
        public async Task Debounce()
        {
            WireSource<int> w = new WireSource<int>();
            int x=0;
            using(w
                .Debug("not debounced")
                .Debounce(100).Debug("debounced")
                .Subscribe(_=>Interlocked.Increment(ref x)))
            {
                w.Emit(1);
                await Task.Delay(20);
                w.Emit(2);
                await Task.Delay(100);
            }
            Assert.Equal(1,x);
        }

        [Fact]
        public async Task DebounceMultithread()
        {
            WireSource<int> w = new WireSource<int>();
            int x = 0;
            using (w
                .Debug("not debounced")
                .Debounce(1).Debug("debounced")
                .Subscribe(_ => Interlocked.Increment(ref x)))
            {
                TaskCompletionSource fence = new TaskCompletionSource();
                for (int i = 0; i < 5; i++)
                {
                    int ii = i;
                    _ = Task.Run(async () =>
                    {
                        await fence.Task;
                        w.Emit(ii);
                    });
                }
                Console.WriteLine("stting result");
                fence.SetResult();
                Console.WriteLine("sat result");
                await Task.Delay(100);
                Console.WriteLine("exiting");
            }
            Assert.Equal(1, x);
        }
        [Fact]
        public async Task AllPkt()
        {
            WireSource<Pkt<bool>>[] w = new WireSource<Pkt<bool>>[4];
            for (int i = 0; i < 4; i++)
                w[i] = new WireSource<Pkt<bool>>();
            var all = w.AllLatest(new Wire.CombineAsyncOptions
            {
                Debounce=1//performance limit
            });
            var b = all.Get();
            bool last = false ;
            for (int i = 0; i < 4; i++)
            {
                last = false;
                w[i].Emit(new (last));
            }
            var val = await b;
            Debug.WriteLine(val);
            Assert.Equal(last, val);
            Assert.False(val);

            b = all.Get();
            for (int i = 0; i < 4; i++)
            {
                last = true;
                w[i].Emit(last);
            }

             val = await b;
            Debug.WriteLine(val);
            Assert.Equal(last, val);
            Assert.True(val);
        }

        [Fact(Timeout=1000)]
        public async Task AllPktMultithread()
        {
            WireSource<Pkt<bool>>[] w = new WireSource<Pkt<bool>>[4];
            for (int i = 0; i < 4; i++)
                w[i] = new WireSource<Pkt<bool>>();
            var all = w.AllLatest(new Wire.CombineAsyncOptions
            {
                Debounce = 1//performance limit
            });
            var b = all.Get();
            TaskCompletionSource fence=new TaskCompletionSource();
            bool last = false;
            for (int i = 0; i < 4; i++)
            {
                last = (false);
                _ = Task.Run(async () =>
                  {
                      await fence.Task;
                      w[i].Emit(last);
                  });
            }
            fence.SetResult();
            var val = await b;
            Debug.WriteLine(val);
            Assert.Equal(last, val);
            Assert.False(val);

            b = all.Get(); fence = new TaskCompletionSource();
            for (int i = 0; i < 4; i++)
            {
                last = (true);
                _ = Task.Run(async () =>
                {
                    await fence.Task;
                    w[i].Emit(last);
                });
            }
            fence.SetResult();

            val = await b;
            Debug.WriteLine(val);
            Assert.Equal(last, val);
            Assert.True(val);
        }
    }
}

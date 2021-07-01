using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GenshinBotTests
{
    public class AsyncPerf:MakeConsoleWork
    {
        public AsyncPerf(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task run()
        {
            int bad = -1;
            int mod = (int)(1e9 + 7);
            async Task<int> recurse(int dep )
            {
                bad = (bad * 3937 + 13) % mod;
                if (dep == 0) return bad;
                var t1 = recurse(dep - 1).ConfigureAwait(false);
                var t2 = recurse(dep - 1).ConfigureAwait(false);
                
                int a=await t1;
                int b =  await t2;
                return (a ^ b);
            }
            int recurse1(int dep )
            {
                bad = (bad * 3937 + 13) % mod;
                if (dep == 0) return bad;
                int a =  recurse1(dep-1);

                int b =  recurse1(dep-1);
                return (a ^ b);
            }
            int dep = 15;
            Stopwatch sw = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();
            int[] res = new int[1000];
            int[] res2 = new int[1000];
            for (int i = 0; i < res.Length; i++)
            {
                sw.Start();
                res[i] = await recurse(dep);
                sw.Stop();

                sw2.Start();
                res2[i] =  recurse1(dep);
                sw2.Stop();
            }
            Console.WriteLine($"async tot={sw.Elapsed/1000} normal tot={sw2.Elapsed/1000}");


            int rr = 0;
            foreach (var r in res) rr ^= r;
            foreach (var r in res2) rr ^= r;
            Console.WriteLine(rr);
        }
    }
}

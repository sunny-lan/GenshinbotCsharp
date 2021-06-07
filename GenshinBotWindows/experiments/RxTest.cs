using genshinbot.reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.experiments
{
    public class RxTest
    {
        public static async Task runAsync()
        {
            Subject<int> thing=new Subject<int>();

            _ = Task.Run(async () =>
            {
                for (int i = 0; ; i++)
                {
                    thing.OnNext(i);
                    await Task.Delay(1000);
                }
            });

            var o1 = thing.Publish().RefCount(); ;

            var o2 = o1.Select(x =>
            {
                Console.WriteLine("processing called");
                return $"o2={x}";
            });

            await o2.Take(1).ObserveOn(Scheduler.Default);

            Console.ReadLine();

            using (o2.Subscribe(x =>
            {
                Console.WriteLine($"output {x}");
            }))
            {
                Console.WriteLine($"subscrbied");
                Console.ReadLine();
                using (o2.Subscribe(x =>
                {
                    Console.WriteLine($"output1 {x}");
                }))
                {
                    Console.WriteLine($"subscrbied1");
                    Console.ReadLine();
                }
            }
            Console.ReadLine();
        }
    }

}


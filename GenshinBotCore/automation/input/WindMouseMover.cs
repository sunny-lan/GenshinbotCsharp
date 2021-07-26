using genshinbot.algorithm;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation.input
{
    public class WindMouseMover : MouseAdapter,  IMouseSimulator2
    {
        algorithm.WindMouseAlg alg;

        public WindMouseMover(IMouseSimulator2 wrap):base(wrap)
        {
            this.alg = new(wrap);
        }

        public override async Task MouseTo(Point2d p)
        {
            await alg.MoveMouse(p.X, p.Y, 0, 0);
        }


        public override async Task MouseMove(Point2d d)
        {
            await MouseTo(d + await MousePos());
        }

        public class Test
        {
            IMouseSimulator2 a;

            public Test(IMouseSimulator2 a)
            {
                this.a =new WindMouseMover( a);
            }
            public async Task TestMove()
            {
                async Task tt(Point pp)
                {

                    var pt = await a.MousePos();
                    Console.WriteLine(pt);
                    await a.MouseMove(pp);
                    var p2 = await a.MousePos();
                    Console.Write(p2 - pt);
                    Console.ReadLine();
                }

                await a.MouseTo(new(0, 0));

                await a.MouseTo(new(1500, 500));
                Console.Write(await a.MousePos());
            }
        }
    }
}

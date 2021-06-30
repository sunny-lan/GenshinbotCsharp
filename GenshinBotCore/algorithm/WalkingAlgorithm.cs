using genshinbot.reactive;
using genshinbot.reactive.wire;
using genshinbot.util;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.algorithm
{
    class WalkingAlgorithm
    {

        public readonly IWire<double> MouseMovements;

        public enum KeyAction
        {
            BeginWalk,
            EndWalk,Drop,
            Jump,None
        }
        public IWire<KeyAction> KeyboardOut { get; }


        public WalkingAlgorithm(
            IWire<Pkt<Point2d>> knownPosition,
            IWire<Pkt<double>> knownAngle,
            IWire<Pkt<bool>> isFlying,
            IWire<Pkt<bool>> isClimbing,
            IWire<Pkt<bool>> isAllDead,


            IWire<Point2d?> wantedPosition
        )
        {
            var arrowControl = new ArrowSteering(knownAngle,
                Wire.CombineLatest(wantedPosition,knownPosition, (wanted, known) =>
                {
                    if (wanted is Point2d pp)
                        return known.Value.AngleTo(pp);
                    else return null as double?;
                })
           );
            LiveWireSource<bool> enableMouse = new(false);
            var otherActions = Wire.CombineLatest(isAllDead, isFlying, isClimbing, (dead, flying, climbing) =>
             {
                 if (dead.Value)
                 {
                     if (flying.Value)
                     {
                         //continue flying
                         enableMouse.SetValue(true);
                        return KeyAction.None;
                     }
                     else if (climbing.Value)
                     {
                         //bad
                         enableMouse.SetValue(false);
                         return KeyAction.Drop;
                     }
                     else
                     {
                         //probably falling
                         enableMouse.SetValue(true);
                         return KeyAction.Jump;
                     }
                 }
                 //grounded
                 enableMouse.SetValue(true);
                 return KeyAction.BeginWalk;
             }).Debounce(200);

            MouseMovements = arrowControl.MouseDelta;

            //todo use more advanced

            //just keep walking as long as wanted position exists
            var goForward = Wire.CombineLatest(knownPosition, wantedPosition,knownAngle,
                (_,x,_) => x is not null); 

            KeyboardOut = goForward.Edge(
                rising:KeyAction.BeginWalk,
                falling:KeyAction.EndWalk
            );

            //jump when in air, press x when climbing if in wrong direction and back up???

            //0. known internal state:
            // walking => climb, fall -> check isdead for statechange
            // falling => press space -> fly, or walk/climb
            // fly 
            //1. detect isdead = could by falling or climbing or flying
            //2. check for climbing with short timeout, if not climbing, then falling
            //3.    while falling, press space
            //4. if climbing
            //5.  check arrow direction
            //6.  if direction wrong, go backwards a bit, repeat step 4

            KeyboardOut = Wire.Merge(KeyboardOut, otherActions
            );

        }
    }
}

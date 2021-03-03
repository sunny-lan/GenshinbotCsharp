using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.input
{
    
    interface IInputSimulator
    {
        Point2d MousePos();
        void MouseMove(Point2d d);
        void MouseTo(Point2d p);

        void MouseDown(int btn);
        void MouseUp(int btn);
        void MouseClick(int btn);


        void KeyDown(int k);

        void KeyUp(int k);

        void KeyPress(int k);
    }
}

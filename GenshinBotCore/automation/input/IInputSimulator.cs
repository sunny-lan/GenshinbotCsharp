using OpenCvSharp;

namespace genshinbot.automation.input
{
    public interface IMouseSimulator
    {

        Point2d MousePos();
        void MouseMove(Point2d d);
        void MouseTo(Point2d p);

        void MouseDown(int btn);
        void MouseUp(int btn);
        void MouseClick(int btn);

        void MouseButton(Point2d pos, int btn, bool down);
    }

    public interface IKeySimulator
    {
        //TODO change everything to use enum
        void KeyDown(int k);

        void KeyUp(int k);

        void KeyPress(int k);
    }
    public interface IInputSimulator:IMouseSimulator,IKeySimulator
    {

        
    }
}

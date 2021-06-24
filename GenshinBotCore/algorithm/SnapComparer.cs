using genshinbot.data;
using OpenCvSharp;

namespace genshinbot.algorithm
{
    public abstract class SnapComparer
    {
        protected Snap Snap { get; }

        protected SnapComparer(Snap snap)
        {
            Snap = snap;
        }

        public abstract double Compare(Mat screen);
    }
}

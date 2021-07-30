using OpenCvSharp;
using System;

namespace genshinbot.algorithm
{
        //TODO switch this to included one
        public class ConnectedComponents
        {
            public enum StatIdx : int
            {
                CC_STAT_AREA = 4,
                CC_STAT_HEIGHT = 3,
                CC_STAT_LEFT = 0,
                CC_STAT_MAX = 5,
                CC_STAT_TOP = 1,
                CC_STAT_WIDTH = 2,
            }
            public struct Stats
            {

                public int Left, Top, Width, Height;
                public int Area;

                // public double cx, cy;
                public Rect Rect => new Rect(Left, Top, Width, Height);
                public int Bottom => Top + Height;
                public int Right => Left + Width;
                public Point P1 => new Point(Left, Top);
                public Point P2 => new Point(Right, Bottom);


                public Size Size => new Size(Width, Height);

            }

            readonly Mat Labels = new Mat(),
                stats = new Mat(),
                centroids = new Mat();
            public int Count { get; private set; } = -1;

            public void CalculateFrom(Mat img)
            {

                Count = Cv2.ConnectedComponentsWithStats(img, Labels, stats, centroids);

            }

            public Stats this[int idx]
            {
                get
                {
                    return new Stats
                    {
                        Left = this[idx, StatIdx.CC_STAT_LEFT],
                        Top = this[idx, StatIdx.CC_STAT_TOP],
                        Width = this[idx, StatIdx.CC_STAT_WIDTH],
                        Height = this[idx, StatIdx.CC_STAT_HEIGHT],
                        Area = this[idx, StatIdx.CC_STAT_AREA],
                    };
                }
            }

            public int this[int idx, StatIdx type]
            {
                get
                {
                    if (Count == -1)
                        throw new Exception("must call CalculateFrom before accessing stats");
                    if (idx >= Count)
                        throw new IndexOutOfRangeException();

                    return stats.Get<int>(idx, (int)type);
                }
            }

            ~ConnectedComponents()
            {
                Labels.Dispose();
                stats.Dispose();
                centroids.Dispose();
            }
        }

}

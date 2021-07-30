using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data
{
    public class Grid
    {

        public Rect2d FirstCell { get; set; }
        public Point2d BottomRight { get; set; }

        public Rect2d WholeRect => Util.RectAround(FirstCell.TopLeft, BottomRight.Add(FirstCell.Size));

        public int Columns { get; set; }
        public int Rows { get; set; }


        public Rect2d Get(int r, int c)
        {
            var tmp = BottomRight;
            if (Rows == 1) tmp.Y = FirstCell.Y;
            if (Columns == 1) tmp.X = FirstCell.X;
            return new Point2d(c, r).Map(
                new(0, 0), new(Columns - 1, Rows - 1),
                FirstCell.TopLeft, tmp
            ).WithSize(FirstCell.Size);
        }

        /*private string?[,]? names;
        public string?[,] Names
        {
            get
            {
                if (names is null)
                    names = new string[Rows,Columns];
                return names;
            }
            set => names = value;
        }
        public IEnumerable<(int r, int c)> Find(Func<string,bool> pred)
        {
            for(int i = 0; i < Rows; i++)
            {
                for(int j = 0; j < Columns; j++)
                {
                    if (Names[i, j] is string s) 
                        if (pred(s))
                            yield return (i, j);
                }
            }
        }

        public Rect2d Get(string name)
        {
            var (r, c) = Find(x => x == name).Single();
            return Get(r, c);
        }*/
    }
}

using OpenCvSharp;
using System;
using System.Threading.Tasks;

/// <summary>
/// Platform independent abstraction of gui
/// </summary>
namespace genshinbot.yui
{
    public static partial class Ext
    {
        public static async Task<int> SelectY(this Viewport v)
        {
            var line = await XYLine.Select(v, Orientation.Horizontal);
            line.Delete();
            return line.Y;
        }
        public static async Task<int> SelectX(this Viewport v)
        {
            var line = await XYLine.Select(v, Orientation.Vertical);
            line.Delete();
            return line.X;
        }
        public static async Task<int> SelectXY(this Viewport v, Orientation orientation)
        {
            var line = await XYLine.Select(v, orientation);
            line.Delete();
            return line.V;
        }
    }

    /// <summary>
    /// A line which is locked to a single X or Y value
    /// </summary>
    public class XYLine
    {
        private Line _line;
        private Orientation orientation;
        private Viewport parent;

        public static XYLine Create(Viewport v, Orientation orientation, int? min = null, int? max = null)
        {
            //TODO
            return new XYLine(orientation, v, min ?? -10000, max ?? 10000);
        }

        public static Task<XYLine> Select(Viewport v, Orientation orientation, int? min = null, int? max = null)
        {
            XYLine line = null;
            var tsk = new TaskCompletionSource<XYLine>();
            var old = v.OnMouseEvent;
            v.OnMouseEvent = evt =>
            {
                if (evt.Type == MouseEvent.Kind.Click)
                {
                    if (line != null)
                    {
                        v.OnMouseEvent = old;
                        tsk.SetResult(line);
                        return true;
                    }
                }
                else if (evt.Type == MouseEvent.Kind.Move)
                {
                    if (line == null) { line = XYLine.Create(v, orientation,min,max); }
                    if (orientation == Orientation.Vertical)
                    {
                        line.X = (int)Math.Round(evt.Location.X);
                    }
                    else
                    {
                        line.Y = (int)Math.Round(evt.Location.Y);
                    }
                    return true;
                }
                return false;
            };
            return tsk.Task;
        }

        public void Delete()
        {
            parent.Delete(this._line);
        }

        private int min, max;

        private XYLine(Orientation orientation, Viewport parent, int min, int max)
        {
            this.min = min;
            this.max = max;
            this._line = parent.CreateLine();
            this.orientation = orientation;
            this.parent = parent;
        }

        int v;
        public int V
        {
            get => v; set
            {
                if (v == value) return;
                v = value;
                if (orientation == Orientation.Horizontal)
                {
                    _line.A = new Point(min, v);
                    _line.B = new Point(max, v);
                }
                else
                {
                    _line.A = new Point(v, min);
                    _line.B = new Point(v, max);
                }
            }
        }
        public int Y
        {
            get
            {
                Debug.Assert(orientation == Orientation.Horizontal);
                return V;
            }
            set
            {
                Debug.Assert(orientation == Orientation.Horizontal);
                V = value;
            }
        }
        public int X
        {
            get
            {
                Debug.Assert(orientation == Orientation.Vertical);
                return V;
            }
            set
            {
                Debug.Assert(orientation == Orientation.Vertical);
                V = value;
            }
        }

    }
}

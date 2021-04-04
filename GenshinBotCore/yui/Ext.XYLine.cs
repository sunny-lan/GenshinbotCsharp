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
    public class XYLine : yui.Component
    {
        private Line _line;
        private Orientation orientation;
        private Viewport vp;

        public static XYLine Create(Viewport v, Orientation orientation, int? min = null, int? max = null)
        {
            //TODO
            return new XYLine(orientation, v, min ?? -10000, max ?? 10000);
        }

        public static Task<XYLine> Select(Viewport v, Orientation orientation, int? min = null, int? max = null)
        {
            XYLine line = null;
            var tsk = new TaskCompletionSource<XYLine>();
            void handleMouseEvent(MouseEvent evt)
            {
                if (evt.Type == MouseEvent.Kind.Click)
                {
                    if (line != null)
                    {
                        v.MouseEvent -= handleMouseEvent;
                        tsk.SetResult(line);
                    }
                }
                else if (evt.Type == MouseEvent.Kind.Move)
                {
                    if (line == null) { line = XYLine.Create(v, orientation, min, max); }
                    if (orientation == Orientation.Vertical)
                    {
                        line.X = (int)Math.Round(evt.Location.X);
                    }
                    else
                    {
                        line.Y = (int)Math.Round(evt.Location.Y);
                    }
                }
            };
            v.MouseEvent += handleMouseEvent;
            return tsk.Task;
        }

        public void Delete()
        {
            vp.Delete(this._line);
        }

        private int min;
        private int max;
        public int Min
        {
            get => min; set
            {
                min = value;
                update();
            }
        }
        public int Max
        {
            get => max; set
            {
                max = value;
                update();
            }
        }
        private XYLine(Orientation orientation, Viewport parent, int min, int max)
        {
            this.min = min;
            this.max = max;
            this._line = parent.CreateLine();
            this.orientation = orientation;
            this.vp = parent;
        }
        private void update()
        {
            if (orientation == Orientation.Horizontal)
            {
                _line.A = new Point(Min, v);
                _line.B = new Point(Max, v);
            }
            else
            {
                _line.A = new Point(v, Min);
                _line.B = new Point(v, Max);
            }
        }

        int v;

        public int V
        {
            get => v; set
            {
                if (v == value) return;
                v = value;
                update();
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


        private bool editable;
        public bool Editable
        {
            get => editable; set
            {
                if (editable != value)
                {
                    editable = value;

                    if (editable)
                    {
                        
                    }
                    else
                    {
                    }
                }
                
            }
        }
        public Action<int> OnVChange { get; set; }

    }
}

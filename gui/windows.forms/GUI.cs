using GenshinbotCsharp.data;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinbotCsharp.yui.WindowsForms
{
    /// <summary>
    /// provides implementation of gui using windows forms
    /// </summary>
    public partial class MainForm : Form, yui.YUI
    {
        public static void Test()
        {
            var _f = new MainForm();
            yui.YUI f = _f;
            var tab = f.CreateTab();
            tab.Title = "test";

            var content = tab.Content;

            var view = content.CreateViewport();
            view.Size = new OpenCvSharp.Size(300, 300);
            var rect = view.CreateRect();
            rect.R = new OpenCvSharp.Rect(10, 20, 30, 40);

            var zoom = content.CreateButton();
            zoom.Text = "+";
            zoom.Click += (s, e) =>
            {
                Console.WriteLine("zoomin");
            };

            var zoom2 = content.CreateButton();
            zoom2.Text = "-";
            zoom2.Click += (s, e) =>
            {
                Console.WriteLine("zoomout");
            };

            Application.Run(_f);
        }

        public MainForm()
        {
            InitializeComponent();
        }

        public yui.Tab CreateTab()
        {
            Tab t = new Tab();
            this.tabs.TabPages.Add(t.control);
            return t;
        }

        class Tab : yui.Tab
        {
            internal TabPage control;
            _Container _children;

            public Tab()
            {
                control = new TabPage();
                _children = new _Container
                {
                    Dock = DockStyle.Fill
                };
                control.Controls.Add(_children);
            }

            public string Title { get => control.Text; set => control.Text = value; }

            public Container Content => _children;

        }

        class Button : System.Windows.Forms.Button, yui.Button
        {

        }

        class _Container : FlowLayoutPanel, Container
        {
            public yui.Button CreateButton()
            {
                var btn = new Button();
                Controls.Add(btn);
                return btn;
            }

            public yui.Viewport CreateViewport()
            {
                var vp = new Viewport();
                Controls.Add(vp);
                return vp;
            }
        }

        class Viewport : Control, yui.Viewport
        {
            private Transformation _transform=new Transformation();

            class _View : Control
            {
                public _View()
                {
                    BackColor = Color.Gray;
                    _controls = new List<Control>();
                    ControlAdded += _View_ControlAdded;
                    ControlRemoved += _View_ControlRemoved;
                }
                List<Control> _controls;
                private void _View_ControlRemoved(object sender, ControlEventArgs e)
                {
                    _controls.Remove(e.Control);
                    RecalcBounds();
                }

                private void _View_ControlAdded(object sender, ControlEventArgs e)
                {
                    _controls.Add(e.Control);
                    RecalcBounds();
                }

                private void RecalcBounds()
                {
                    Rectangle newbounds = new Rectangle();
                    for (int i = 0; i < Controls.Count; i++)
                    {
                        var control = Controls[i];
                        Console.WriteLine(control.Bounds);
                        newbounds = newbounds.Union(control.Bounds);
                    }
                    Console.WriteLine(newbounds);
                    this.Bounds = newbounds;
                }

                protected override bool ScaleChildren => true;

            }




            OpenCvSharp.Size yui.Viewport.Size { get => Size.cv(); set => Size = value.Sys(); }


            public Transformation Transformation
            {
                get => _transform;
                set
                {
                    
                    _transform = value;
                    for(int i = 0; i < Controls.Count; i++)
                    {
                        var control = Controls[i];
                        Recalc(control);
                    }
                }
            }

            

            void Recalc(Control control)
            {
                var scale = new SizeF((float)_transform.Scale, (float)_transform.Scale);
                control.Location = _transform.Transform(Util.Origin).Round().Sys();
                control.Scale(scale);
            }

            public Viewport()
            {
                BackColor = Color.Black;
                ControlAdded += (s, e) => {
                    Recalc(e.Control);
                };
            }
            public yui.Image CreateImage()
            {
                var i = new Image();
                Controls.Add(i);
                return i;
            }

            public yui.Rect CreateRect()
            {
                var r = new Rect();
                Controls.Add(r);
                return r;
            }
        }

        interface ViewportComponent
        {
            OpenCvSharp. Point BaseLocation { get;  }
        }

        class Rect : Control, yui.Rect, ViewportComponent
        {
            private OpenCvSharp.Rect _r;

            public bool Editable { get; set; }


            public OpenCvSharp.Rect R
            {
                get => _r;// => Bounds.Cv();
                set{
                    _r = value;
                    Size = _r.Size.Sys();
                }// => Bounds = value.Sys();
            }

            public OpenCvSharp.Point BaseLocation => R.TopLeft;

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.DrawRectangle(Pens.Red, Bounds);
            }
        }

        class Image : PictureBox, yui.Image, ViewportComponent
        {
            Mat _img;
            Bitmap bmp;

            /// <summary>
            /// must not be disposed before the class is disposed
            /// </summary>
            public Mat image
            {
                get => _img;
                set
                {
                    _img = value;
                    bmp = value.ToBmpFast();

                }
            }
            public OpenCvSharp.Point TopLeft
            {
                get => this.Location.Cv();
                set => this.Location = value.Sys();
            }

            public OpenCvSharp.Point BaseLocation => throw new NotImplementedException();
        }
    }
}

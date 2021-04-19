using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.yui.tools
{
    class GenericDbEditor : GenericDbEditor.Processor
    {
        public interface Processor
        {
            bool process(object o, yui.TreeView.Node nd, Action<object> onChange, Processor processor);
        }

        class TypeProcessor : Processor
        {
            interface I<T>
            {
                bool CanInit { get; }

                void Edit(yui.Container container, Action<T> onEdit);

                string ToString(T v);
            }
            Dictionary<Type, I<object>> pp;
            public bool process(object o, TreeView.Node nd, Action<object> onChange, Processor processor)
            {
                var t = o.GetType();
                if (!pp.ContainsKey(t)) return false;

                I<object> ppp = pp[t];



                return true;
            }
        }
        class ListProcessor : Processor
        {
            public bool process(object o, TreeView.Node nd, Action<object> onChange, Processor processor)
            {
                var t = o.GetType();
                if (o is IList l)
                {

                    for (int i = 0; i < l.Count; i++)
                    {
                        var n = nd.CreateChild();
                        n.Text = i.ToString();
                        if (l[i] == null)
                        {
                            n.Text += "=null";
                            continue;
                        }

                        processor.process(l[i], n, x =>
                        {
                            l[i] = x;
                            if (t.IsValueType)
                                onChange(l);
                        }, processor);
                    }
                    return true;
                }
                return false;
            }
        }

        class RectProcessor : Processor
        {
            private GenericDbEditor e;

            public RectProcessor(GenericDbEditor e)
            {
                this.e = e;
            }

            public bool process(object o, TreeView.Node nd, Action<object> onChange, Processor processor)
            {
                if(o is OpenCvSharp.Rect r)
                {
                    nd.Text += " " + r.ToString();
                    nd.DoubleClick += (s, e) =>
                    {
                       Task.Run(()=> this.e.EditRect(null,x=> onChange(x)));
                    };
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class RDProcessor : Processor
        {
            GenshinBot b;
            private GenericDbEditor e;
            public Mat screenshot;
            event EventHandler onScreenshotChange;
            public RDProcessor(GenericDbEditor e)
            {
                b = e.b;
                this.e = e;

            }
            public bool process(object o, yui.TreeView.Node nd, Action<object> onChange, Processor processor)
            {
                if (o is IDictionary d)
                {
                    var t = o.GetType();
                    if (!t.IsGenericType) return false;
                    var args = t.GenericTypeArguments;
                    if (args.Length != 2) return false;
                    if (args[0] != typeof(Size)) return false;



                    onScreenshotChange += (s, e) =>
                    {
                        this.e.pg.BeginUpdate();
                        nd.ClearChildren();
                        if (screenshot == null)
                        {
                            nd.Text = "Resolution dependent";
                            nd.Color = Scalar.Yellow;
                        }
                        else
                        {
                            nd.Color = Scalar.Black;
                            var sz = screenshot.Size();
                            nd.Text = sz.ToString() + " ";
                            var sub = d[sz];
                            var ret = processor.process(sub, nd, x => d[sz] = x, processor);
                            Dbg.Assert(ret);
                        }
                        this.e.pg.EndUpdate();
                    };

                    EventHandler onSelect = (s, e) =>
                    {
                        this.e.SetImg(screenshot);
                        var btn = this.e.editTab.Content.CreateButton();
                        btn.Text = "Take screenshot";
                        EventHandler onClk = (s, e) =>
                        {
                            screenshot = b.W.Screenshot(b.W.GetBounds()).CvtColor(ColorConversionCodes.BGRA2BGR);
                            this.e.SetImg(screenshot);
                            onScreenshotChange?.Invoke(s, e);
                        };
                        btn.Click += onClk;
                        nd.Deselected += (s, e) =>
                        {
                            btn.Click -= onClk;
                            this.e.editTab.Content.Delete(btn);
                        };
                    };
                    nd.Selected += onSelect;

                    return true;
                }
                return false;
            }
        }

        private GenshinBot b;
        private Tab editTab;
        private Viewport vp;
        private yui.Image img;
        Rect r;
         void SetImg(Mat img)
        {
            this.img.Mat = img;
        }
        private void EditRect(OpenCvSharp.Rect? initValue, Action<OpenCvSharp.Rect> onChange)
        {
            if (r != null)
            {
                vp.Delete(r);
                r = null;
            }

            if (initValue is OpenCvSharp.Rect r2)
            {
                r = vp.CreateRect();
                r.R = r2;
            }
            else
            {
                var t = vp.SelectCreateRect();
                t.Wait();
                r = t.Result;
                onChange(r.R);
            }


        }


        public GenericDbEditor(GenshinBot b)
        {
            this.b = b;
            processors = new List<Processor>
            {
                new ListProcessor(),
                new RDProcessor(this),
                new RectProcessor(this),
            };
            makeEditTab();
        }
        void makeEditTab()
        {
            this.editTab = b.Ui.CreateTab();
            this.vp = editTab.Content.CreateViewport();
            vp.Size = new Size(500, 500);
            vp.OnTChange = x => vp.T = x;
            img = vp.CreateImage();
             this.pg = editTab.Content.CreateTreeview();

            pg.BeginUpdate();
            var root = pg.CreateNode();
            process(b.Db, root, (x) => throw new NotImplementedException(), this);
            pg.EndUpdate();
        }

        List<Processor> processors;
        private TreeView pg;

        void Edit(string val, Action<string> onChange)
        {

        }

        public bool process(object o, TreeView.Node nd, Action<object> onChange, Processor processor)
        {
            Dbg.Assert(processor == this);

            foreach (var p in processors)
            {
                if (p.process(o, nd, onChange, processor)) return true;
            }
            var t = o.GetType();

            var props = t.GetProperties();
            foreach (var prop in props)
            {
                var n = nd.CreateChild();
                n.Text = prop.Name + ": " + prop.PropertyType.Name;

                var idxParams = prop.GetIndexParameters();
                if (idxParams.Length > 0)
                {
                    n.Text += "=unsupported";
                    n.Color = Scalar.Red;
                    continue;
                }

                var v = prop.GetValue(o);
                if (v == null)
                {
                    n.Text += "=null";
                    n.Color = Scalar.Yellow;
                    continue;
                }

                this.process(v, n, x =>
                {
                    prop.SetValue(o, x);
                    if (t.IsValueType)
                        onChange(o);
                    n.Invalidate();
                }, this);
            }
            return true;
        }
    }
}

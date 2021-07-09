﻿using genshinbot.automation;
using genshinbot.automation.hooking;
using genshinbot.automation.input;
using genshinbot.data;
using genshinbot.data.events;
using genshinbot.diag;
using genshinbot.reactive;
using genshinbot.reactive.wire;
using OneOf;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static genshinbot.reactive.Ext;

namespace genshinbot.tools
{
    public class AutofillTool
    {
        private IWindowAutomator2 w;
        private ILiveWire<IReadOnlyDictionary<Keys, bool>> kk;
        private yui.windows.Overlay2 overlay;
        abstract class Filler
        {
            public abstract Task<object> Fill(object o);
            public abstract Type Type { get; }

            class Impl<T> : Filler<T>
            {
                public Func<T, Task<T>> action { get; init; }
                public override Task<T> FillT(T o) => action(o);
            }
            public static Filler<T> From<T>(Func<T, Task<T>> f)
            {
                return new Impl<T> { action = f };
            }
        }

        abstract class Filler<T> : Filler
        {
            public override Type Type => typeof(T);

            public override async Task<object> Fill(object o)
            {
                if (o is T t)
                {
                    return await FillT(t);

                }
                else
                {

                    throw new Exception();
                }
            }
            public abstract Task<T> FillT(T o);


        }

        private void Prompt(string s, int idx = -1)
        {
            lock (overlay.Text)
                if (idx >= 0)
                    overlay.Text.Push(s);
            Indent.Println(s);
        }
        private void ClearPrompt(int idx = -1)
        {
            lock (overlay.Text)
                if (idx >= 0)
                    overlay.Text.Pop();
        }

        class KeyComb
        {
            public Keys[] Keys;
            public string Description;
        }

        KeyComb Select = new KeyComb
        {
            Keys = new[] { Keys.OemSemicolon },
            Description = ";"
        };
        KeyComb Cancel = new KeyComb
        {
            Keys = new[] { Keys.N },
            Description = "N"
        };
        KeyComb Left = new KeyComb
        {
            Keys = new[] { Keys.OemOpenBrackets },
            Description = "["
        }; KeyComb Right = new KeyComb
        {
            Keys = new[] { Keys.OemCloseBrackets },
            Description = "]"
        };

        private async Task<KeyComb> WaitComboAsync(params KeyComb[] cmb)
        {
            //TODO KSETCH
            return await w.KeyCap.KeyEvents
                .Where(   e => 
                !e.Down)
                .Select(e => cmb.Where(cmb => cmb.Keys[0] == e.Key).ToArray())
                .Where(x => x.Length == 1).Select(x => x[0]).Get();
        }

        public AutofillTool(IWindowAutomator2 w)
        {
            this.w = w;
            this.kk = w.KeyCap.KbdState.Do(x =>
            {
                foreach (var k in x) Console.Write(k.Value ? k.Key : "" + ",");
                Console.WriteLine();
            });
            var point2DFiller = Filler.From<Point2d>(async x =>
            {
                var sz = await w.Bounds.Value2();
                Prompt($"{Select.Description} to select, {Cancel.Description} to cancel", 1);
                Point2d pos = x;
                KeyComb key;
                Point2d? last=null;
                void setPos(Point2d p)
                {
                    if (sz.Contains(p.Round()))
                    {
                        pos = p;
                        overlay.Point = pos.Round();
                    }
                }
                int TOTAL = 1; 
                using (w.MouseCap.MouseEvents.Subscribe( evt =>
                {
                    //ignore if alt is not pressed!
                    if(System.Windows.Forms.Cursor.Current is not null)
                    if (evt is MoveEvent mEvt)
                    {

                           setPos( mEvt.Position);

                    }
                }))
                using (w.KeyCap.KeyEvents.Subscribe(st =>
                {
                    if (st.Down)
                    {
                        int v = TOTAL;
                        TOTAL++;
                        TOTAL = Math.Min(TOTAL, 10);

                        Point2d pos2 = pos;
                        if (st.Key == Keys.Left) pos2 += new Point2d(-v, 0);
                        if (st.Key == Keys.Right) pos2 += new Point2d(v, 0);
                        if (st.Key == Keys.Up) pos2 += new Point2d(0, -v);
                        if (st.Key == Keys.Down) pos2 += new Point2d(0, v);


                        setPos(pos2);
                    }
                    else
                    {
                        TOTAL = 1;
                    }
                }))
                {
                    key = await WaitComboAsync(Select, Cancel);
                }

                ClearPrompt(1);
                overlay.Point = null;
                if (key == Select)
                {
                    return pos;
                }
                else return x;

            });
            var pointFiller = Filler.From<Point>(async x =>
            {
                return (await point2DFiller.FillT(x)).Round();
            });
            var rect2dFiller = Filler.From<Rect2d>(async x =>
              {
                  using (Indent.Inc())
                  {
                      //show original
                      overlay.Rect = x.round();

                      Prompt("Top left", 1);
                      var tl = await point2DFiller.FillT(x.TopLeft);
                      ClearPrompt(1);
                      Prompt("Bottom right", 1);
                      var br = await point2DFiller.FillT(x.BottomRight);
                      ClearPrompt(1);

                      var res = Util.RectAround(tl, br);
                      overlay.Rect = res.round();

                      Prompt($"{Select.Description} to return, {Cancel.Description} to revert", 1);
                      var key = await WaitComboAsync(
                         Select,
                         Cancel
                      );
                      ClearPrompt(1);

                      overlay.Rect = null;
                      if (key == Select)
                      {
                          return res;
                      }
                      else if (key == Cancel)
                      {
                          return x;
                      }
                      else Debug.Assert(false);
                      return x;
                  }
              });
            var rectFiller = Filler.From<Rect>(async x =>
            {
                var r = await rect2dFiller.FillT(x.cvt());
                return r.round();
            });
            var snapFiller = Filler.From<data.Snap>(async x =>
            {
                overlay.Visible = false;
                var img = await w.Screen.Watch2(w.Bounds).Depacket().Get();
                overlay.Visible = true;

                img = img.Clone();
                overlay.Image = img;

                var r = await rectFiller.FillT(x.Region);
                overlay.Image = null;

                return new data.Snap
                {
                    //TODO we need to hide the overlay when taking a shot
                    Image = new SavableMat { Value = img[r] },
                    Region = r,
                };
            });

            var matFiller = Filler.From<SavableMat>(async x =>
            {
                return (await snapFiller.FillT(new data.Snap { })).Image;
            });
            Filler[] fillers1 =
            {
                rectFiller,
                rect2dFiller,
                pointFiller,
                point2DFiller,
                matFiller,
                snapFiller
                //TODO enum
            };
            fillers = new Dictionary<Type, Filler>();
            foreach (var filler in fillers1)
                fillers[filler.Type] = filler;

            overlay = new yui.windows.Overlay2();
            overlay.run();
            disp.Add(overlay.follow(w.Focused));
            disp.Add(overlay.follow(w.ScreenBounds));
            
        }
        List<IDisposable> disp = new List<IDisposable>();
        ~AutofillTool()
        {
            overlay.Dispose();
            foreach (var d in disp)
                d.Dispose();
        }



        Dictionary<Type, Filler> fillers;
        async Task<object> edit(object o, Type tt, string path = "")
        {
            using (Indent.Inc())
            { var under = Nullable.GetUnderlyingType(tt);
                if ( under!= null)
                {
                    return await edit(o, under, path);
                }
                if (o == null)
                {
                    Prompt("object is null. try to construct", 1);
                    ClearPrompt(1);

                        o = Activator.CreateInstance(tt);


                    if (o == null)
                    {
                        Prompt("WARN: unable to construct, skip");

                        return o;
                    }
                }
                if (fillers.TryGetValue(tt, out var filler))
                {
                    Prompt($"{path} - {tt.Name}: {o}", 0);
                    var newval = await filler.Fill(o);
                    ClearPrompt(0);

                    Prompt($"{path} - new: {newval}");
                    return newval;
                }
                var stuffs = new List<(Func<object?> o, string subpath, Action<object> set, Type t)>();
                if (o is Array arr)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var icpy = i;
                        stuffs.Add((
                            () => arr.GetValue(icpy),
                            $"{path}[{icpy}]",
                            o => arr.SetValue(o, icpy),
                            tt.GetElementType()!
                        ));
                    }
                }else if(o is IDictionary d)
                {
                    var args = tt.GetGenericArguments();
                    Type? subt = null;
                    if (args.Length == 2)
                    {
                        subt = args[1];
                    }
                    foreach (var key in d.Keys)
                    {
                        var kk = key;
                        var ttt =
                            d[kk]?.GetType() ?? subt;
                        if(ttt is not null)
                        stuffs.Add((
                            ()=>d[kk],
                            $"{path}[{kk}]",
                            o=>d[kk]=o,ttt
                        ));
                    }
                }
                else
                {

                    var props = tt.GetProperties().Where(prop => prop.CanWrite && prop.CanRead).ToList();
                    foreach (var prop in props)
                    {
                        var subpath = $"{path}.{prop.Name}";
                        stuffs.Add((
                            () => prop.GetValue(o),
                            subpath,
                            x => prop.SetValue(o, x),
                            prop.PropertyType
                        ));
                    }

                }
                if (stuffs.Count == 1)
                {
                    var val = stuffs[0];
                    val.set(await edit(val.o(), val.t, val.subpath));
                    return o;
                }
                int idx = 0;
                while (true)
                {
                    var subpath = stuffs[idx].subpath;
                    Prompt($"{stuffs[idx].t.Name} {subpath} = {stuffs[idx].o() ?? "null"}", 0);
                    Prompt($"{Left.Description}/{Right.Description} to inc/dec. {Select.Description} to select. {Cancel.Description} to exit.", 1);
                    var key = await WaitComboAsync(Select, Cancel, Left, Right);
                    ClearPrompt(1);
                    ClearPrompt(0);
                    if (key == Select)
                    {
                        var val = stuffs[idx];
                        val.set(await edit(val.o(), val.t, subpath));
                        //idx = (idx + 1) % stuffs.Count;
                    }
                    else if (key == Cancel)
                    {
                        return o;
                    }
                    else if (key == Left)
                    {
                        idx = (idx - 1 + stuffs.Count) % stuffs.Count;
                    }
                    else if (key == Right)
                    {
                        idx = (idx + 1) % stuffs.Count;
                    }
                    else Debug.Assert(false);
                }
            }
        }
        
        public async Task Edit(object o)
        {
            using (Indent.Inc())
            {
                var props = o.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var type = prop.PropertyType;
                    var val = prop.GetValue(o);
                    //check for RD dict
                    if (val is IDictionary d)
                    {
                        if (type.IsGenericType)
                        {
                            var args = type.GetGenericArguments();
                            if (args.Length == 2 && args[0] == typeof(Size))
                            {

                                var sz = await w.Size.Value2();
                                //ensure window size doesn't change while doing RD
                                await w.Size.NonNull().Lock(async () =>
                                    {
                                        var rd = d[sz];
                                        Prompt($"{prop.Name}:{sz.Width}x{sz.Height}", 0);

                                        d[sz] = await edit(rd, args[1], prop.Name);
                                        ClearPrompt(0);
                                    },sz);
                            }
                        }
                    }
                }
            }
        }


         async Task roulette( List<(string description, Func<Task> inner)> stuffs)
        {
            int idx = 0;
            while (true)
            {
                Prompt($"{stuffs[idx].description}", 0);
                Prompt($"{Left.Description}/{Right.Description} to inc/dec. {Select.Description} to select. {Cancel.Description} to exit.", 1);
                var key = await WaitComboAsync(Select, Cancel, Left, Right);
                ClearPrompt(1);
                ClearPrompt(0);
                if (key == Select)
                {
                    await stuffs[idx].inner();
                }
                else if (key == Cancel)
                {
                    return;
                }
                else if (key == Left)
                {
                    idx = (idx - 1 + stuffs.Count) % stuffs.Count;
                }
                else if (key == Right)
                {
                    idx = (idx + 1) % stuffs.Count;
                }
                else Debug.Assert(false);
            }
        }
        public async Task ConfigureAll()
        {
            Console.WriteLine("searching assembly for DB");
            var ass = AppDomain.CurrentDomain.GetAssemblies();
                List<(string description, Func<Task> inner)> lst = new();
            foreach (var a in ass)
            {
                var types = a.GetTypes();

                foreach (var t in types)
                {
                    if (t.IsGenericType) continue;//TODO support for generic db types
                    //Console.WriteLine(t.Name);
                    foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (f.FieldType.IsAssignableTo(typeof(DbInst)))
                        {
                            lst.Add((
                                $"{t.FullName}.{f.Name}",
                                async () =>
                                {
                                    var db = (DbInst)f.GetValue(null);
                                    await Edit(db.ObjVal);
                                    Prompt($"{Select.Description} to save. {Cancel.Description} to cancel.", 1);
                                    var key = await WaitComboAsync(Select, Cancel);
                                    ClearPrompt(1);
                                    if (key == Select)
                                    {
                                        await db.Save();
                                    }
                                }
                            ));
                        }
                    }
                }
            }
            await roulette(lst);
        }
        public static async Task Test(ITestingRig r)
        {
            var rig = r.Make();
            var tool = new AutofillTool(rig.W);
            await tool.Edit(new screens.PlayingScreen.Db());
        }

        class TestObj
        {
            public class Peep
            {
                public Point[] advanced { get; set; } = new Point[4];

            }
            public class RD
            {
                public Peep peep { get; set; }

                public SavableMat derp { get; set; }
                public data.Snap derp1 { get; set; }
                //public Point2d p2d { get; set; }

                // public Rect poo { get; set; }


            }
            public Dictionary<Size, RD> Rd { get; set; } = new Dictionary<Size, RD>();
        }

        public static async Task Test2()
        {
            var notepad = new automation.windows.WindowAutomator2("*Untitled - Notepad", null);
            var tool = new AutofillTool(notepad);
            var obj = new TestObj();
            await tool.Edit(obj);
            foreach (var val in obj.Rd.Values)
            {
                CvThread.ImShow("a", val.derp.Value);
                CvThread.ImShow("b", val.derp1.Image.Value);
            }
        }

        public static async Task ConfigureDispatch(BotIO w)
        {
            var tool = new AutofillTool(w.W);
            await tool.Edit(DispatchDb.Instance);
            await DispatchDb.SaveInstanceAsync();
        }
        public static async Task ConfigureCharacterSel(BotIO w)
        {
            var tool = new AutofillTool(w.W);
            await tool.Edit(CharacterSelectorDb.Instance.Value);
            await CharacterSelectorDb.Instance.Save();
        }
        public static async Task ConfigAll(BotIO w)
        {
            var tool = new AutofillTool(w.W);
            await tool.ConfigureAll();
        }

        public static async Task ConfigurePlayingScreen(BotIO w)
        {
            var tool = new AutofillTool(w.W);
            await tool.Edit(screens.PlayingScreen.Db.Instance);
            await screens.PlayingScreen.Db.SaveInstanceAsync();
        }


    }
}

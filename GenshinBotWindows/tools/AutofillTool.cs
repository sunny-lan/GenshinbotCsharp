﻿using genshinbot.automation;
using genshinbot.automation.hooking;
using genshinbot.automation.input;
using genshinbot.diag;
using genshinbot.reactive;
using OneOf;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class AutofillTool
    {
        private IWindowAutomator2 w;
        private IObservable<IReadOnlyDictionary<Keys, bool>> kk;
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


        Filler<T> ComboFiller<T>(Func<Task<T>> get)
        {
            return Filler.From(async (T x) =>
            {
                Prompt("Ctrl-J to select");
                await kk.KeyCombo(Keys.LControlKey, Keys.J).Get();
                ClearPrompt();
                return await get();
            });
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

        public AutofillTool(IWindowAutomator2 w)
        {
            this.w = w;
            this.kk = w.KeyCap.KbdState;
            var point2DFiller = Filler.From<Point2d>(async x =>
            {
                //TODO move logic to overlay
                while (true)
                {
                    Prompt("Ctrl-J to select, Ctrl-L to skip", 1);
                    Point2d pos = x;
                    string key;
                    using (w.MouseCap.MouseEvents.Subscribe(onNext: evt =>
                    {
                        if (evt is IMouseCapture.MoveEvent mEvt)
                        {
                            overlay.Point = pos.Round();
                            pos = mEvt.Position;
                        }
                    }))
                    {
                        key = await await Task.WhenAny(
                            kk.KeyCombo(Keys.LControlKey, Keys.J).Select(_ => "select").Get(),
                            kk.KeyCombo(Keys.LControlKey, Keys.L).Select(_ => "skip").Get()


                            );
                    }
                    ClearPrompt(1);
                    if (key == "skip")
                    {
                        return x;
                    }
                    Prompt("Enter to return, Ctrl-0 to retry", 1);
                    key = await await Task.WhenAny(
                       kk.KeyCombo(Keys.Enter).Select(x => "go").Get(),
                       kk.KeyCombo(Keys.LControlKey, Keys.D0).Select(x => "retry").Get()
                   );
                    ClearPrompt(1);
                    if (key == "go")
                    {
                        overlay.Point = null;
                        return pos;
                    }
                    else if (key == "retry")
                    {
                        continue;
                    }
                    else Debug.Assert(false);
                }

            });
            var pointFiller = Filler.From<Point>(async x =>
            {
                return (await point2DFiller.FillT(x)).Round();
            });
            var rect2dFiller = Filler.From<Rect2d>(async x =>
              {
                  using (Indent.Inc())
                  {
                      while (true)
                      {
                          Prompt("Top left", 1);
                          var tl = await point2DFiller.FillT(x.TopLeft);
                          ClearPrompt(1);
                          Prompt("Bottom right", 1);
                          var br = await point2DFiller.FillT(x.BottomRight);
                          ClearPrompt(1);

                          var res = Util.RectAround(tl, br);
                          overlay.Rect = res.round();

                          Prompt("Enter to return, Ctrl-0 to retry", 1);
                          var key = await await Task.WhenAny(
                              kk.KeyCombo(Keys.Enter).Select(x => "go").Get(),
                              kk.KeyCombo(Keys.LControlKey, Keys.D0).Select(x => "retry").Get()
                          );
                          ClearPrompt(1);

                          overlay.Rect = null;
                          if (key == "go")
                          {
                              return res;
                          }
                          else if (key == "retry")
                          {
                              continue;
                          }
                          else Debug.Assert(false);
                      }
                  }
              });
            var rectFiller = Filler.From<Rect>(async x =>
            {
                var r = await rect2dFiller.FillT(x.cvt());
                return r.round();
            });
            var matFiller = Filler.From<Mat>(async x =>
            {
                var r = await rectFiller.FillT(default);
                return await w.Screen.Watch(r).Depacket().Get();
            });
            var snapFiller = Filler.From<data.Snap>(async x =>
            {
                var r = await rectFiller.FillT(x.Region);
                return new data.Snap
                {
                    //TODO we need to hide the overlay when taking a shot
                    Image = await w.Screen.Watch(r).Depacket().Get(),
                    Region = r,
                };
            });

            Filler[] fillers1 =
            {
                rectFiller,
                rect2dFiller,
                pointFiller,
                point2DFiller,
                matFiller,
                snapFiller
            };
            fillers = new Dictionary<Type, Filler>();
            foreach (var filler in fillers1)
                fillers[filler.Type] = filler;

        }

        Dictionary<Type, Filler> fillers;
        async Task<object> edit(object o, Type tt, string path = "")
        {
            using (Indent.Inc())
            {
                if (o == null)
                {
                    Prompt("object is null. Ctrl-5 to construct. Ctrl-0 to skip", 1);
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
                var props = tt.GetProperties();
                foreach (var prop in props)
                {
                    if (prop.CanWrite && prop.CanRead)
                    {
                        var val = prop.GetValue(o);
                        var subpath = $"{path}.{prop.Name}";

                        Prompt($"{prop.Name}");
                        prop.SetValue(o, await edit(val, prop.PropertyType, subpath));
                    }
                }
            }
            return o;
        }

        public async Task Edit(object o)
        {
            overlay = new yui.windows.Overlay2();
            overlay.run();
            using (overlay.follow(w.Focused))
            using (overlay.follow(w.ScreenBounds))
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
                                var sz = await w.Size.Get();
                                var rd = d[sz];
                                Prompt($"{prop.Name}:{sz.Width}x{sz.Height}", 0);

                                d[sz] = await edit(rd, args[1], prop.Name);
                                ClearPrompt(0);
                            }
                        }
                    }
                }
            }
            overlay.Dispose();
            overlay = null;
        }
        public static async Task Test(ITestingRig r)
        {
            var rig = r.Make();
            var tool = new AutofillTool(rig.W);
            await tool.Edit(new screens.PlayingScreen.Db());
        }

        class TestObj
        {
            public class RD
            {
                public Mat derp { get; set; }
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
            foreach(var val in obj.Rd.Values)
            {
                CvThread.ImShow("a",val.derp);
                CvThread.ImShow("b",val.derp1.Image);
            }
        }


        public static async Task ConfigureDailyDoer(BotIO w)
        {
            var tool = new AutofillTool(w.W);
            await tool.Edit(DispatchDb.Instance.Value);
            await DispatchDb.SaveInstanceAsync();
        }

    }
}
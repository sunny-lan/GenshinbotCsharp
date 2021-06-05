using genshinbot.automation;
using genshinbot.automation.hooking;
using genshinbot.automation.input;
using genshinbot.diag;
using genshinbot.reactive;
using OneOf;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.tools
{
    public class AutofillTool
    {
        private IWindowAutomator2 w;
        private IObservable<IReadOnlyDictionary<Keys, bool>> kk;

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
                await kk.KeyCombo(Keys.LControlKey, Keys.J).Get();
                return await get();
            });
        }


        public AutofillTool(IWindowAutomator2 w)
        {
            this.w = w;
            this.kk = w.KeyCap.KbdState;
            var point2DFiller = ComboFiller(() => w.Mouse.MousePos());
            var pointFiller = Filler.From<Point>(async x =>
            {
                return (await point2DFiller.FillT(x)).Round();
            });
            var rect2dFiller = Filler.From<Rect2d>(async x =>
              {
                  using (Indent.Inc())
                  {
                      Indent.Println("Top left");
                      var tl = await point2DFiller.FillT(x.TopLeft);
                      Indent.Println("Bottom right");
                      var br = await point2DFiller.FillT(x.BottomRight);
                      return Util.RectAround(tl, br);
                  }
              });
            var rectFiller = Filler.From<Rect>(async x =>
            {
                using (Indent.Inc())
                {
                    Indent.Println("Top left");
                    var tl = await pointFiller.FillT(x.TopLeft);
                    Indent.Println("Bottom right");
                    var br = await pointFiller.FillT(x.BottomRight);
                    return Util.RectAround(tl, br);
                }
            });

            Filler[] fillers1 =
            {
                rectFiller,
                rect2dFiller,
                pointFiller,
                point2DFiller,
            };
            fillers = new Dictionary<Type, Filler>();
            foreach (var filler in fillers1)
                fillers[filler.Type] = filler;

        }

        Dictionary<Type, Filler> fillers;
        async Task<object> edit(object o)
        {
            using (Indent.Inc())
            {
                var tt = o.GetType();
                if (o == null)
                {
                    Indent.Println("object is null. Ctrl-5 to construct. Ctrl-0 to skip");

                    o=Activator.CreateInstance(tt);
                    if (o==null)
                    {
                        Indent.Println("WARN: unable to construct, skip");

                        return o;
                    }
                }
                var props = tt.GetProperties();
                foreach (var prop in props)
                {
                    if (prop.CanWrite && prop.CanRead)
                    {
                        var val = prop.GetValue(o);
                        if (fillers.TryGetValue(prop.PropertyType, out var filler))
                        {
                            Indent.Println($"{prop.Name} - old: {val}");
                            var newval = await filler.Fill(val);
                            prop.SetValue(o, newval);
                            Indent.Println($"{prop.Name} - new: {newval}");

                        }
                        else
                        {
                            Indent.Println($"{prop.Name}");
                            prop.SetValue(o, await edit(val));
                        }
                    }
                }
            }
            return o;
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
                                var sz = await w.Size.Get();
                                var rd = d[sz];
                                Indent.Println(prop.Name);
                                
                                d[sz] = await edit(rd);
                            }
                        }
                    }
                }
            }
        }
        public static async Task Test(ITestingRig r)
        {
            var rig = r.Make();
            var tool = new AutofillTool(rig.W);
            await tool.Edit(new screens.PlayingScreen.Db());
        }

    }
}

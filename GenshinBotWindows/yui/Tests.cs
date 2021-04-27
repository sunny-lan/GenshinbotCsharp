using genshinbot.data;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace genshinbot.yui
{
    static class Tests
    {
        public static void GenericTest(YUI f)
        {

            var tab = f.CreateTab();
            tab.Title = "test";

            var content = tab.Content;

            var view = content.CreateViewport();
            view.Size = new Size(1000, 600);
            view.OnTChange = t => view.T = t;

            var kk = view.CreateImage();
            kk.Mat = Data.Imread("map/genshiniodata/assets/MapExtracted_12.png");

            var rect = view.CreateRect();
            rect.R = new OpenCvSharp.Rect(100, 100, 30, 40);




            var zoom = content.CreateButton();
            zoom.Text = "+";
            zoom.Click += (s, e) =>
            {
                Console.WriteLine("zoomin");
                var r = view.T;
                r.Scale *= 1.1;
                view.T = r;
            };

            var zoom2 = content.CreateButton();
            zoom2.Text = "-";
            zoom2.Click += (s, e) =>
            {
                Console.WriteLine("zoomout");
                var r = view.T;
                r.Scale *= 0.9;
                view.T = r;
            };


           Task.Run(()=> { while (true) view.SelectCreateRect().Wait(); });
        }
    }
}
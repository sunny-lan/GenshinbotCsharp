﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Math;

namespace GenshinbotCsharp.screens
{
    class MapScreen : Screen
    {
        private GenshinBot b;
        private Screenshot.Buffer buf;

        public Mat Map => buf.Mat;

        public MapScreen(GenshinBot b)
        {
            this.b = b;

            initLocator();

        }

        public void Close()
        {
            b.W.K.KeyPress(input.GenshinKeys.Map);
            Thread.Sleep(2000);//TODO
            b.S(b.PlayingScreen);
        }

        public bool CheckActive()
        {
            throw new NotImplementedException();
        }


        public algorithm.MapLocationMatch LocationMatch { get; private set; }
        public algorithm.MapTemplateMatch TemplateMatch { get; private set; }

        private void initLocator()
        {
            LocationMatch = new algorithm.MapLocationMatch(b.Db.MapDb.Features);
            TemplateMatch = new algorithm.MapTemplateMatch();
        }

        public void AddFeature(database.map.Feature f)
        {
            b.Db.MapDb.Features.Add(f);
            LocationMatch.AddFeature(f);
        }

        private List<algorithm.MapTemplateMatch.Result> features;
        private algorithm.MapLocationMatch.Result location;

        public void UpdateScreenshot()
        {
            var r = b.W.GetRect();
            if (buf==null  || r.Size != buf.Size)
            {
                buf = Screenshot.GetBuffer(r.Width, r.Height);
            }

            b.W.TakeScreenshot(0, 0, buf);

            features = null;
            location = null;
        }

        public List<algorithm.MapTemplateMatch.Result> GetFeatures()
        {
            if (features == null)
                features = TemplateMatch.FindTeleporters(buf.Mat).ToList();
            return features;
        }

        public bool ExpectUnknown = true;

        public algorithm.MapLocationMatch.Result GetLocation()
        {
            if (location == null)
                location = LocationMatch.FindLocation2(GetFeatures(), b.W.GetRect().Size.cv(), ExpectUnknown);
            return location;
        }

        /// <summary>
        /// Returns the screen point of a coordinate. If the point is not on screen, it will try to move towards the point
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public Point2d ShowOnScreen(Point2d coord)
        {
            while (true)
            {
                UpdateScreenshot();
                var l = GetLocation();
                var point = l.ToPoint(coord);
                var r = b.W.GetBounds().Cv();
                if (r.Contains(point.ToPoint()))
                {
                    return point;
                }
                var center = r.Center();
                b.W.I.MouseTo(center);
                b.W.K.KeyDown(input.GenshinKeys.Attack);
                b.W.I.MouseMove((center - point).LimitDistance(100));
                b.W.K.KeyUp(input.GenshinKeys.Attack);
                //TODO
            }
        }

    }
}

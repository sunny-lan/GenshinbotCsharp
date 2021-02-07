﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace GenshinbotCsharp.algorithm
{
    class MapLocationMatch
    {
        class Pair : IComparable<Pair>
        {
            public double Angle;
            public Feature A;
            public Feature B;

            public int CompareTo(Pair other)
            {
                return Angle.CompareTo(other.Angle);
            }

            public Pair(Feature a, Feature b)
            {
                A = a;
                B = b;
                Angle = a.Coordinates.AngleTo(b.Coordinates);
            }

            public double Distance => A.Coordinates.DistanceTo(B.Coordinates);
        }

        private List<Pair> allPairs = new List<Pair>();
        private List<Feature> allFeatures = new List<Feature>();


        public void AddFeature(Feature a)
        {
            foreach (var b in allFeatures)
            {
                var pair = new Pair(a, b);
                if (pair.Angle < 0)
                    pair = new Pair(b, a);
                if (pair.Angle < 0)
                    throw new Exception("assert failed");
                allPairs.Add(pair);
            }

            allFeatures.Add(a);
            allPairs.Sort();
        }
        double anglediff(double a, double b)
        {
            var diff = Abs(a - b);
            return Min(diff, 2 * PI - diff);
        }
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public class Result
        {
            public class Match
            {
                public MapTemplateMatch.Result A;
                public Feature B;
                public double Distance;

            }
            public Point2d Translation;
            public double Scale;
            public List<MapTemplateMatch.Result> Unknown;
            public List<Match> Matches;
            public double Score;

            public Point2d ToCoord(Point2d pt)
            {
                return pt * Scale + Translation;
            }

            public Point2d ToPoint(Point2d coord)
            {
                return (coord - Translation) * (1 / Scale);
            }
        }

        const double ANGLE_TOLERANCE = PI / 18;
        const double COORD_DIST_TOLERANCE =100;
        const double MIN_ACCEPTABLE_SCORE = 0.9;
        const int MAX_RETRIES = 4;
        public const int MAX_EXPECTED_FALSE_POSITIVES = 2;
        public const double MAX_EXPECTED_FALSE_NEGATIVE_RATE =0;
        Size pb;

        void TestTransform(Point2d translation, double scale, List<MapTemplateMatch.Result> list,bool expectUnknown,ref Result result,Size bound )
        {
            if (pb.Width != 0)
            {
                if (pb != bound) throw new Exception();
            }
            pb = bound;
            var unknown = new List<MapTemplateMatch.Result>();

            var l2 = list.Select(x => new Result.Match
            {
                A=x,
                B=null,
                Distance=double.PositiveInfinity,
            }).ToList();

            int inRect = 0, matched=0;
            foreach (var t in allFeatures) //200
            {
                Point2d screen = (t.Coordinates - translation) * (1 / scale);
                if (screen.X < 0 || screen.Y < 0 || screen.X > bound.Width || screen.Y > bound.Height) { }else
                inRect++;
                foreach(var m in l2)
                {
                    double d = screen.DistanceTo(m.A.Point);
                    if (d > COORD_DIST_TOLERANCE) continue;
                    if (d < m.Distance)
                    {
                        if (m.B == null)
                            matched++;
                        m.B = t;
                        m.Distance = d;
                        
                    }
                }
            }
            if (matched<=2 )

            {
                return;
            }
            if(matched / (double)inRect < MAX_EXPECTED_FALSE_NEGATIVE_RATE)
            {
                return;
            }
            double curScor = 1;
            int count = 0;
            foreach (var m in l2)
            {
                if (m.B==null)
                {

                    //unable to match
                    unknown.Add(m.A);
                    if (expectUnknown == false
                        && unknown.Count > MAX_EXPECTED_FALSE_POSITIVES)
                        return;
                }
                else
                {
                    count++;
                    curScor *= Util.scoreFunc(0, m.Distance  / COORD_DIST_TOLERANCE);
                }
            }


            //geometric mean of individual distance scores
            curScor = Pow(curScor, 1.0 / count);
            if (curScor > MIN_ACCEPTABLE_SCORE && curScor > result.Score)
            {
                result.Score = curScor;
                result.Translation = translation;
                result.Scale = scale;
                result.Matches = l2;
                result.Unknown = unknown;
            }
        }
        public Result FindLocation2(List<MapTemplateMatch.Result> list21,Size bounds, bool expectUnknown = true)
        {
            if (allPairs.Count == 0)
            {
                return new Result
                {
                    Score = 0,
                    Unknown = list21,
                    Scale = 1,
                    Translation = new Point2d(0, 0),
                };
            }

            var list = new List<MapTemplateMatch.Result>(list21);
            list.Sort((x, y) => x.Point.X.CompareTo(y.Point.X));
            
            for (int i = 0, j=list.Count-1;i<j;i++,j--)
            {
                var a = list[i].Point;
                var b = list[j].Point;
                double angle = a.AngleTo(b);
                if (angle < 0)
                {
                    Swap(ref a, ref b);
                    angle = a.AngleTo(b);
                }
                //special case: when there are not data, we just return the whole input list
                //as unknown 
                var result = new Result
                {
                    Score = 0,
                    Unknown = null,
                    Scale = double.NaN,
                    Translation = new Point2d(double.NaN, double.NaN),
                };

                //check every pair of teleporters
                //TODO implement binary search
                foreach (var candidate in allPairs) //200^2
                {
                    if (anglediff(angle, candidate.Angle) > ANGLE_TOLERANCE) continue;

                    //calculate the transformation of this candidate
                    //conversion from screen to map 
                    double scale = candidate.Distance / a.DistanceTo(b);
                    Point2d translation = candidate.A.Coordinates - a * scale;

                    TestTransform(translation, scale, list, expectUnknown, ref result,bounds);
                }
                if (result.Score > MIN_ACCEPTABLE_SCORE)
                {
                    return result;
                }
            }
            throw new Exception("unable to find location");
        }

        public Result FindLocation(IEnumerator<MapTemplateMatch.Result> list, bool shortCircuit = true)
        {
            //get the first two teleporters
            bool flag = false;
            Point2d a = default, b = default;
            if (list.MoveNext())
            {
                a = list.Current.Point;
                if (list.MoveNext())
                {
                    b = list.Current.Point;
                    flag = true;
                }
            }


            Cv2.NamedWindow("result", WindowMode.KeepRatio);
            if (flag)
            {
                //calculate the angle between the two
                /*var diff = b - a;
                var angle = Atan2(diff.Y, diff.X);
                if (angle < 0)
                {
                    Swap(ref b, ref a);
                    diff = b - a;
                    angle = Atan2(diff.Y, diff.X);
                }*/

                //find all pairs of teleporters with a similar angle
                var candidates = allPairs;
                /* var candidates = new List<Pair>();
                 foreach (var x in allPairs)
                 {

                      if (anglediff(x.Angle, angle) < ANGLE_MATCH_TOLERANCE)
                     {
                         candidates.Add(x);
                     }
                 }*/

                //look for more teleporters until we can narrow down the possible transformations to 1

                while (list.MoveNext() && (!shortCircuit || candidates.Count > 1))
                {
                    var teleporterPos = list.Current;

                    var nextCandiates = new List<Pair>();

                    //narrow down list of possible candidates
                    foreach (var candidate in candidates)
                    {
                        //calculate the transformation of this candidate
                        //conversion from screen to map 
                        double scale = candidate.Distance / a.DistanceTo(b);
                        Point2d translation = candidate.A.Coordinates - a * scale;

                        //transform the found teleporter position to map coordinates
                        Point2d mapCoords = teleporterPos.Point * scale + translation;

                        //find the closest teleporter
                        double closest = double.PositiveInfinity;
                        foreach (var teleporterM in allFeatures)
                        {
                            closest = Min(closest, teleporterM.Coordinates.DistanceTo(teleporterPos.Point));
                        }



                    }

                    candidates = nextCandiates;
                }

                if (candidates.Count == 0)
                    throw new Exception("no candidates!");

                double scaleFinal = candidates[1].Distance / a.DistanceTo(b);
                Point2d translationFinal = candidates[1].A.Coordinates - a * scaleFinal;

                foreach (var teleporterM in Data.Map.Teleporters)
                {
                    Point2d screenCoords = (teleporterM.Coordinates - translationFinal) * (1 / scaleFinal);

                }

            }


            return new Result();
        }
    }
}

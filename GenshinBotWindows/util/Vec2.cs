using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.util
{
    struct Vec2
    {
        public double X, Y;

        public Vec2(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static readonly Vec2 Origin = new Vec2(0, 0);

        public double Magnitude => Math.Sqrt(X * X + Y * Y);

        public double Angle => Math.Atan2(Y,X);

        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator -(Vec2 a) => new Vec2(-a.X, -a.Y);
        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator *(Vec2 a, double b) => new Vec2(a.X * b, a.Y * b);
        public static Vec2 operator /(Vec2 a, double b) => new Vec2(a.X / b, a.Y / b);

        public double DistanceTo(Vec2 b)
        {
            return (this - b).Magnitude;
        }
    }
}

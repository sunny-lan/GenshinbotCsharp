using OpenCvSharp;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace genshinbot.database.jsonconverters
{
    //TODO make internal classes vector2d and such
    class Point2dConverter : JsonConverter<Point2d>
    {
        struct Point
        {
            public double X { get; set; }
            public double Y { get; set; }
        }
        public override Point2d Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var x=JsonSerializer.Deserialize<Point>(ref reader, options);
            return new Point2d(x.X, x.Y);
        }

        public override void Write(Utf8JsonWriter writer, Point2d value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<Point>(writer, new Point { X = value.X, Y = value.Y }, options);
        }
    }

    class PointConverter : JsonConverter<Point>
    {
        struct Pointa
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var x = JsonSerializer.Deserialize<Pointa>(ref reader, options);
            return new Point(x.X, x.Y);
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<Pointa>(writer, new Pointa { X = value.X, Y = value.Y }, options);
        }
    }

    class RectConverter : JsonConverter<Rect>
    {
        struct Recta
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
        public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var x = JsonSerializer.Deserialize<Recta>(ref reader, options);
            return new Rect(x.X, x.Y,x.Width,x.Height);
        }

        public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<Recta>(writer, new Recta { 
                X = value.X, Y = value.Y ,
                Width=value.Width,Height=value.Height,
            }, options);
        }
    }

    class Rect2dConverter : JsonConverter<Rect2d>
    {
        struct Recta
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
        public override Rect2d Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var x = JsonSerializer.Deserialize<Recta>(ref reader, options);
            return new Rect2d(x.X, x.Y, x.Width, x.Height);
        }

        public override void Write(Utf8JsonWriter writer, Rect2d value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<Recta>(writer, new Recta
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height,
            }, options);
        }
    }
}
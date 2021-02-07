using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GenshinbotCsharp.database.jsonconverters
{
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
}

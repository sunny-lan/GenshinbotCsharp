using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{



    static class Data
    {

        private static JsonSerializerOptions options = new JsonSerializerOptions()
        {
            Converters =
            {
                new database.jsonconverters.Point2dConverter()
            },
            IgnoreNullValues = true,
        };
        public static string Get(string name)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data", name);
        }

        public static bool Exists(string name)
        {
            return File.Exists(Get(name));
        }

        public static string Read(string name)
        {
            return File.ReadAllText(Get(name));
        }

        public static void Write(string name, string value)
        {
            File.WriteAllText(Get(name), value);
        }

        public static T ReadJson<T>(string name, T def)
        {
            if (!Exists(name))
                return def;
            return JsonSerializer.Deserialize<T>(Read(name), options);
        }

        public static T ReadJson<T>(string name)
        {

            return JsonSerializer.Deserialize<T>(Read(name), options);
        }
        public static void WriteJson<T>(string name, T data)
        {
            Write(name, JsonSerializer.Serialize<T>(data, options));
        }

        public static Mat Imread(string name, ImreadModes mode = ImreadModes.Color)
        {
            var path = Get(name);
            if (!File.Exists(path))
                throw new Exception("File doesn't exist " + path);
            var res = Cv2.ImRead(path, mode);
            if (res.Empty())
                throw new Exception("Read empty image");
            return res;
        }

    }
}

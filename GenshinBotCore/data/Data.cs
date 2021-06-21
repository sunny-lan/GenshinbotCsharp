using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace genshinbot.data
{



    public static partial class Data
    {

        private static JsonSerializerOptions options = new JsonSerializerOptions()
        {
            Converters =
            {
                new jsonconverters.Point2dConverter(),
                new jsonconverters.PointConverter(),

                new jsonconverters.RDConverterFactory(),

                new jsonconverters.Rect2dConverter(),
                new jsonconverters.RectConverter(),
                new jsonconverters.ScalarConverter(),
                new jsonconverters.MatConverter(),

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

        public static void Write(string name, string value, bool createDirs = true)
        {
            var path = Get(name);
            if (createDirs)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllText(path, value);
        }

        public static async Task WriteAsync(string name, string value, bool createDirs = true)
        {
            var path = Get(name);
            if (createDirs)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            await File.WriteAllTextAsync(path, value);
        }

        public static T ReadJson<T>(string name, T def)
        {
            if (!Exists(name))
                return def;
            return JsonSerializer.Deserialize<T>(Read(name), options);
        }
        public static T ReadJson1<T>(string name)where T:new(){
            return ReadJson(name, new T());
        }
        public static T ReadJson<T>(string name)
        {

            return JsonSerializer.Deserialize<T>(Read(name), options);
        }
        public static void WriteJson<T>(string name, T data)
        {
            Write(name, JsonSerializer.Serialize(data, options));
        }
        public static async Task WriteJsonAsync<T>(string name, T data)
        {
            await WriteAsync(name, JsonSerializer.Serialize(data, options));
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
        public static void Imwrite(string name, Mat m, bool createDirs = true)
        {
            var path = Get(name);
            var dir = Path.GetDirectoryName(path);
            if (createDirs)
            {
                Directory.CreateDirectory(dir);
            }
            if (!Directory.Exists(dir))
                throw new Exception("Folder doesn't exist " + dir);
            Debug.Assert(Cv2.ImWrite(path, m),"imwrite failed");
        }

    }
}

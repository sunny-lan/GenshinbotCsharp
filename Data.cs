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
    class Feature
    {
        public Point2d Coordinates;

    }

    class Teleporter : Feature
    {
        public string Name;
    }

    class MapData
    {
        public List<Teleporter> Teleporters=new List<Teleporter>();
    }

    static class Data
    {
       static List<Teleporter> ReadTeleporters(string region)
        {
            var result = new List<Teleporter>();
            var json = Json<data.json.MapData>(Path.Combine("map/json/features",region,"special/teleporter.json"));
            foreach(var teleporter in json.data)
            {
                
                result.Add(new Teleporter
                {
                    Coordinates=new Point2d(teleporter.geometry.coordinates[1],-teleporter.geometry.coordinates[0]),
                    Name=teleporter.properties.popupTitle.en,
                    
                });
            }
            return result;
        }

        public static MapData Map { get; private set; }
        static Data()
        {
            Map = new MapData();
           // Map.Teleporters.AddRange(ReadTeleporters("mondstadt"));
            Map.Teleporters.AddRange(ReadTeleporters("liyue"));
            //Map.Teleporters.AddRange(ReadTeleporters("dragonspine"));
        }

        public static string Get(string name)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"data",name);
        }

        public static string Read(string name)
        {
            return File.ReadAllText(Get(name));
        }

        public static T Json<T>(string name)
        {
            return JsonSerializer.Deserialize<T>(Read(name));
        }

        public static Mat Imread(string name, ImreadModes mode=ImreadModes.Color)
        {
            var path = Get(name);
            if (!File.Exists(path))
                throw new Exception("File doesn't exist");
            var res= Cv2.ImRead(path, mode);
            if (res.Empty())
                throw new Exception("Read empty image");
            return res;
        }

    }
}

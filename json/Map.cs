namespace GenshinbotCsharp.data.json
{

    public class MapData
    {
        public Name name { get; set; }
        public bool cluster { get; set; }
        public Icons icons { get; set; }
        public Datum[] data { get; set; }
    }

    public class Name
    {
        public string en { get; set; }
    }

    public class Icons
    {
        public string filter { get; set; }
        public Base _base { get; set; }
        public Done done { get; set; }
    }

    public class Base
    {
        public string key { get; set; }
        public int[] iconSize { get; set; }
        public int[] shadowSize { get; set; }
        public int[] iconAnchor { get; set; }
        public int[] shadowAnchor { get; set; }
        public int[] popupAnchor { get; set; }
    }

    public class Done
    {
        public string key { get; set; }
        public int[] iconSize { get; set; }
        public int[] shadowSize { get; set; }
        public int[] iconAnchor { get; set; }
        public int[] shadowAnchor { get; set; }
        public int[] popupAnchor { get; set; }
    }

    public class Datum
    {
        public int id { get; set; }
        public Geometry geometry { get; set; }
        public string type { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public double[] coordinates { get; set; }
    }

    public class Properties
    {
        public Popuptitle popupTitle { get; set; }
        public Popupcontent popupContent { get; set; }
        public string popupMedia { get; set; }
    }

    public class Popuptitle
    {
        public string en { get; set; }
    }

    public class Popupcontent
    {
        public string en { get; set; }
    }

}
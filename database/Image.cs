using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.database
{
    class Image
    {
        public string Path { get; set; }

        private Mat _cached;
        public Mat Load(ImreadModes mode = ImreadModes.Color)
        {
            if (_cached != null) return _cached;
            return _cached = Data.Imread(Path, mode);
        }
    }
}

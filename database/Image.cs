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

        public Mat Load(ImreadModes mode = ImreadModes.Color)
        {
            return Data.Imread(Path, mode);
        }
    }
}

﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.data
{
    public class Image
    {
        public string Path { get; set; }

        private Lazy<Mat> mat;
        private ImreadModes prevMode;
        public Mat Load(ImreadModes mode = ImreadModes.Color)
        {
            if (!Data.Exists(Path)) return null;
            if(mode != prevMode)
            {
                prevMode = mode;
                mat = null;
            }
            if (mat == null)
            {
                mat = new Lazy<Mat>(() => Data.Imread(Path, mode));
            }
            return mat.Value;
        }

        public void Save(Mat m)
        {
            mat = new Lazy<Mat>(m);
            Data.Imwrite(Path, m);
        }
    }
}
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp.screens
{
    class LoadingScreenDb
    {
        public class RD
        {
            public Rect Patch { get; internal set; } = new Rect(10, 10, 20, 20);

            public Scalar Dark { get; set; }
            public Scalar Light { get; set; }
        }

        public Dictionary<Size, RD> R { get; set; }
    }

    class LoadingScreen:Screen
    {
        //TODO
        private LoadingScreenDb db = new LoadingScreenDb();
        private GenshinWindow w;

        public bool CheckActive()
        {
            throw new NotImplementedException();
        }

        void checkPatch()
        {
            var sz = w.GetSize();
            var patch = db.R[sz].Patch;
            


        }
    }
}

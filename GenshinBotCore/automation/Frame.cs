using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation
{
    interface Frame
    {
        /// <summary>
        /// Get a subframe
        /// </summary>
        /// <param name="subrect"></param>
        /// <returns></returns>
         Frame this[Rect subrect] {  get; }

        /// <summary>
        /// return the actual screenshot
        /// </summary>
        /// <returns></returns>
        public Mat Take();

    }
}

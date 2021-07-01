using genshinbot.memo;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation.screenshot
{
    public interface IScreenshotMemo
    {
        Mem<Mat> Take(Rect r); 
    }
}

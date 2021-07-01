using genshinbot.memo;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation.screenshot
{
    public class SubrectMemoHelper : IScreenshotMemo
    {
        Dictionary<Rect, ValueSource<Mat>> cache=new();
        public void OnChange(Rect r)
        {

        }
        public Mem<Mat> Take(Rect r)
        {
            
            if(!cache.TryGetValue(r, out var val))
            {

            }
            throw new NotImplementedException();

        }
    }
}

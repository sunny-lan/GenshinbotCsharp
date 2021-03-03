using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{

    class Player
    {
        private Recording r;
        private int idx;
        public event EventHandler<Event> OnEvent;
        private TimeSpan offset;
        private DateTime start;
        public bool Playing { get; private set; } = false;
        public Player(Recording r)
        {
            this.r = r;
            Reset();
        }
        public void Reset()
        {
            if (Playing) throw new Exception("cannot reset while playing");
            idx = 0;
            offset = TimeSpan.Zero;
        }

        public void Play()
        {
            if (Playing) throw new Exception("already playing");
            Playing = true;
            start = DateTime.Now;
            //WARNING: UBER CPU USAGE
            while (Playing && idx < r.Records.Count)
            {
                var tnow = (DateTime.Now - start) + offset;
                var tevt = r.Records[idx].Time;
                if (tnow > tevt)
                {
                    OnEvent?.Invoke(this, r.Records[idx].Evt);
                    idx++;
                }
                else if (tevt - tnow > TimeSpan.FromMilliseconds(50))
                {
                    Thread.Sleep((int)((tevt - tnow).Milliseconds * 0.9));
                }
            }
            Playing = false;
        }

        public void Pause()
        {
            if (!Playing) throw new Exception("already paused");
            offset += (DateTime.Now - start);
            Playing = false;
        }
    }
}

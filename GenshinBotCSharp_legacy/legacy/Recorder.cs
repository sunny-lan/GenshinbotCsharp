using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    class Recorder
    {
        public Recording Rec;
        private DateTime start;
        private TimeSpan offset;

        public Recorder()
        {
            Clear();
        }

        public bool Recording { get; private set; } = false;

        public void Clear()
        {
            if (Recording) throw new Exception("cannot clear while recording");
            Rec = new Recording();
            offset = TimeSpan.Zero;
        }

        public void Start()
        {
            if (Recording) throw new Exception("already recording");
            Recording = true;
            start = DateTime.Now;
        }

        public void OnEvent(Event r)
        {
            if (!Recording) return;

            Rec.Records.Add(new Record
            {
                Time = (DateTime.Now - start) + offset,
                Evt = r,
            });
        }

        public void Stop()
        {
            if (!Recording) throw new Exception("already stopped");
            Recording = false;
            offset += DateTime.UtcNow - start;

        }
    }
}

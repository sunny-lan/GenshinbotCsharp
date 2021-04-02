using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace GenshinbotCsharp
{
    abstract class Event
    {
        public enum EvtType : byte
        {
            MOUSE = 6, KEYBOARD = 7,WINDOW=8,
        }


        public static Event From(BinaryReader br)
        {
            var typ = (EvtType)br.ReadByte();
           if (typ == EvtType.KEYBOARD)
                return KeyboardEvent.From(br);
            else throw new Exception("invalid fmt");
        }
        public void To(BinaryWriter bw)
        {
            bw.Write((byte)Type);
            SubTo(bw);
        }
        protected abstract void SubTo(BinaryWriter bw);
        public abstract EvtType Type { get; }


    }

    abstract class MouseEvent : Event
    {

        public override Event.EvtType Type => Event.EvtType.MOUSE;
    }

    class KeyboardEvent : Event
    {
        public VirtualKeyCode KeyCode;
        public enum KbEvtType : byte
        {
            DOWN = 8, UP = 9
        }

        public KbEvtType KbType;

        public override Event.EvtType Type => Event.EvtType.KEYBOARD;

        public static new KeyboardEvent From(BinaryReader br)
        {
            var x = new KeyboardEvent();

            x.KeyCode = (VirtualKeyCode)br.ReadInt32();
            x.KbType = (KbEvtType)br.ReadByte();
            return x;
        }

        protected override void SubTo(BinaryWriter bw)
        {
            bw.Write((int)KeyCode);
            bw.Write((byte)KbType);
        }
    }

    class WindowEvent : Event
    {
        public override EvtType Type => EvtType.WINDOW;

        public static WindowEvent From(BinaryReader br)
        {
            throw new NotImplementedException();
        }

        protected override void SubTo(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }


    }


    class Record
    {
        public Event Evt;
        public TimeSpan Time;

        public static Record From(BinaryReader br)
        {
            var x = new Record();


            x.Time = new TimeSpan(br.ReadInt64());
            x.Evt = Event.From(br);
            return x;
        }

        public void To(BinaryWriter bw)
        {
            bw.Write(Time.Ticks);
            Evt.To(bw);
        }
    }

    class Recording
    {
        public List<Record> Records = new List<Record>();
        public static Recording From(BinaryReader br)
        {
            int len = br.ReadInt32();
            var Records = new List<Record>(len);
            for (int i = 0; i < len; i++)
            {
                Records.Add(Record.From(br));
            }
            return new Recording { Records = Records };
        }

        public void To(BinaryWriter bw)
        {
            bw.Write(Records.Count);
            foreach (Record record in Records)
                record.To(bw);
        }
    }
}

using genshinbot.automation.input;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation
{
    class ArduinoAutomator:IDisposable,input.IMouseSimulator2,input.IKeySimulator2
    {
        private SerialPort sp;
        private Func<Task<Point2d>> getMousePos;

        ArduinoAutomator(string port,Func<Task<Point2d>> getMousePos)
        {
             sp=new SerialPort(port, 2000000);
            sp.Open();
            this.getMousePos = getMousePos;
        }


        public void Dispose() => sp.Dispose();

        enum Command : byte
        {
            key_down = (byte)'k',
            mouse_move = (byte)'m',
            key_up = (byte)'j',
            mouse_down = (byte)'c',
            mouse_up = (byte)'d',
            toggle_mk = (byte)'t',
            release_all = (byte)'r'
        }

        public async Task Key(Keys k, bool down)
        {
            byte[] buf = { (byte)(down ?Command.key_down:Command.key_up), (byte)k };
            await sp.BaseStream.WriteAsync(buf, 0, buf.Length);
            await sp.BaseStream.FlushAsync();
        }

        public async Task<Point2d> MousePos() => await getMousePos();

        public async Task MouseMove(Point2d d)
        {
            byte[] buf = { (byte)(Command.mouse_move), (byte)d.X,(byte)d.Y ,0};
            await sp.BaseStream.WriteAsync(buf, 0, buf.Length);
            await sp.BaseStream.FlushAsync();

        }

        public async Task MouseTo(Point2d p)
        {
            var d = p - await MousePos();
            byte[] buf = { (byte)(Command.mouse_move), (byte)d.X, (byte)d.Y, 0 };
            await sp.BaseStream.WriteAsync(buf, 0, buf.Length);
            await sp.BaseStream.FlushAsync();
            throw new NotImplementedException();
        }

        public async Task MouseButton(MouseBtn btn, bool down)
        {
            byte[] buf = { (byte)(down ? Command.mouse_down: Command.mouse_up), (byte)(btn switch{
               MouseBtn.Left=>'l',
               MouseBtn.Middle=>'m',
               MouseBtn.Right=>'r', _ => throw new ArgumentException(), } )};
            await sp.BaseStream.WriteAsync(buf, 0, buf.Length);
            await sp.BaseStream.FlushAsync();
        }

        ~ArduinoAutomator() => Dispose();
    }
}

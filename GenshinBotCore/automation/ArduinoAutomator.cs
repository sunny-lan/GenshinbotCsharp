using genshinbot.automation.input;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace genshinbot.automation
{
    public class ArduinoAutomator : IDisposable, input.IMouseSimulator2, input.IKeySimulator2
    {
        private SerialPort sp;
        private Func<Task<Point2d>> getMousePos;
        private Task init;
        public ArduinoAutomator(string port, Func<Task<Point2d>> getMousePos)
        {
            sp = new SerialPort(port, 2000000);

            sp.DataBits = 8;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.Handshake = Handshake.None;
            sp.DtrEnable = true;
            sp.RtsEnable = true;
            sp.Open();
            this.getMousePos = getMousePos;
            async Task init_fn()
            {
                await _send(new byte[] {
                    (byte)Command.release_all,

                });
            }
            init = init_fn();
        }
        public ArduinoAutomator(Func<Task<Point2d>> getMousePos) : this(SerialPort.GetPortNames()[0], getMousePos) { }

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
        Dictionary<Keys, byte> overrides=new()
        {
            [Keys.Escape]=0xb1,
            [Keys.F1]=0xc2,
            [Keys.F2]=0xc3,
            [Keys.F3]=0xc4,
            [Keys.F4]=0xc5,

        };
        public async Task Key(Keys k, bool down)
        {
            byte kk = overrides.GetValueOrDefault(k, (byte)k);
            byte[] buf = { (byte)(down ? Command.key_down : Command.key_up), kk };
            await send(buf);
        }
        public double Scaling = 150.0 / 100;
        public async Task<Point2d> MousePos() => await getMousePos();
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MouseMoveCommand
        {
            public Command Command;
            public short X, Y, Z;

            public MouseMoveCommand(short x, short y, short z)
            {
                Command = Command.mouse_move;
                Y = y;
                X = x;
                Z = z;
            }

            public byte[] Serialize()
            {
                // Get the byte size of a MyDataStruct structure if it is to be
                // marshaled to unmanaged memory.
                Int32 iSizeOMyDataStruct = Marshal.SizeOf<MouseMoveCommand>();
                // Allocate a byte array to contain the bytes of the unmanaged version
                // of the MyDataStruct structure.
                byte[] byteArrayMyDataStruct = new byte[iSizeOMyDataStruct];
                // Allocate a GCHandle to pin the byteArrayMyDataStruct array
                // in memory in order to obtain its pointer.
                GCHandle gch = GCHandle.Alloc(byteArrayMyDataStruct, GCHandleType.Pinned);
                // Obtain a pointer to the byteArrayMyDataStruct array in memory.
                IntPtr pbyteArrayMyDataStruct = gch.AddrOfPinnedObject();
                // Copy all bytes from the managed MyDataStruct structure into
                // the byte array.
                Marshal.StructureToPtr(this, pbyteArrayMyDataStruct, false);
                // Unpin the byteArrayMyDataStruct array in memory.
                gch.Free();
                // Return the byte array.
                // It contains the serialized bytes of the MyDataStruct structure.
                return byteArrayMyDataStruct;
            }
        }
        Point2d adjustorFactor;
        public async Task MouseMove(Point2d d)
        {
            d *= 1 / Scaling;

            await send(new MouseMoveCommand((short)d.X, (short)d.Y, 0).Serialize());

        }
        public double Accuracy = 1;
        public async Task MouseTo(Point2d p)
        {
            var d = p - await MousePos();

            await MouseMove(d);
        }

        async Task send(byte[] buf)
        {
            await init;
            await _send(buf);
        }
        async Task _send(byte[] buf)
        {
            byte[] buf1 = new byte[1];

           // Console.WriteLine(Convert.ToHexString(buf));
            await sp.BaseStream.WriteAsync(buf, 0, buf.Length);
            await sp.BaseStream.FlushAsync();
            await sp.BaseStream.ReadAsync(buf1, 0, 1);
            if (buf1[0] != (byte)'a')
            {
                string msg = ""+(char)buf1[0];
                while (sp.BytesToRead > 0) msg += (char)sp.ReadChar();
                throw new Exception(msg);
            }
        }

        public async Task MouseButton(MouseBtn btn, bool down)
        {
            byte[] buf = { (byte)(down ? Command.mouse_down: Command.mouse_up), (byte)(btn switch{
               MouseBtn.Left=>'l',
               MouseBtn.Middle=>'m',
               MouseBtn.Right=>'r', _ => throw new ArgumentException(), } )};
            await send(buf);
        }

        ~ArduinoAutomator() => Dispose();

        public class Test
        {
            ArduinoAutomator a;

            public Test(ArduinoAutomator a)
            {
                this.a = a;
            }
            public async Task TestMove()
            {
                async Task tt(Point pp)
                {

                    var pt = await a.MousePos();
                    Console.WriteLine(pt);
                    await a.MouseMove(pp);
                    var p2 = await a.MousePos();
                    Console.Write(p2 - pt);
                    Console.ReadLine();
                }

                var pp = new Point(100,-500);// Util.ReadPoint();
                //await tt(new Point(100, -100));
                //await tt(new Point(100, 500));
                await tt(pp);
                await a.MouseTo(new(500, 500));
                Console.Write(await a.MousePos());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace GenshinBotTests
{
    [DebuggerNonUserCode]
    public class MakeConsoleWork : TraceListener, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly TextWriter _originalOut;
        private readonly TextWriter _textWriter;
        string tmp="";

        public MakeConsoleWork(ITestOutputHelper output):base()
        {
            _output = output;
            _originalOut = Console.Out;
            _textWriter = new StringWriter();
            Console.SetOut(_textWriter);
            Trace.Listeners.Add(this);
        }

        void IDisposable.Dispose()
        {
            _output.WriteLine(_textWriter.ToString());
            Console.SetOut(_originalOut);
            base.Dispose();
        }

        public override void Write(string? message)
        {
            lock(_output)
            tmp += message;
        }

        public override void WriteLine(string? message)
        {
            lock (_output)
            {
                _output.WriteLine(tmp + message);
                tmp = "";
            }
        }
    }

}

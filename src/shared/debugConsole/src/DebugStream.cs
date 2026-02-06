using System.Data.Common;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text;

namespace lib.debug;
public class DebugStreamWriter : StreamWriter
{
    string LogFileName;
    public DebugStreamWriter(NamedPipeServerStream stream, bool leaveOpen = false) : base(stream, leaveOpen: leaveOpen) { }
    public DebugStreamWriter(NamedPipeClientStream stream, bool leaveOpen = false) : base(stream, leaveOpen: leaveOpen) { }


    public void SetFile(string file)
    {
        LogFileName = file;
    }
    public override void WriteLine(string value)
    {
        AddToLogFile(value);
        base.WriteLine(value);
        Flush();
    }

    public override void WriteLine()
    {
        WriteLine(string.Empty);
    }

    public override void WriteLine(object value)
    {
        WriteLine(value?.ToString());
    }

    public void AddToLogFile(string message)
    {
        if (!string.IsNullOrEmpty(LogFileName))
        {
            File.AppendAllText($"logs/debug_{LogFileName}.log", $"W {DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}", Encoding.UTF8);
        }
    }
}

public class DebugStreamReader : StreamReader
{
    public string LogFileName;
    public DebugStreamReader(NamedPipeServerStream stream, bool leaveOpen = false)
        : base(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: leaveOpen) { }
        public DebugStreamReader(NamedPipeClientStream stream, bool leaveOpen = false)
        : base(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: leaveOpen) { }

    public void SetFile(string file)
    {
        LogFileName = file;
        if (!string.IsNullOrEmpty(LogFileName))
        {
            AddToLogFile(LogFileName);
        }
    }

    public override string ReadLine()
    {
        if (EndOfStream)
        {
            return null;
        }
        char c = (char)base.Peek();
        if (c == '\0')
        {
            return null;
        }
        string line = base.ReadLine();
        if (line != null)
        {
            if (!string.IsNullOrEmpty(LogFileName))
            {
                AddToLogFile(line);
            }
        }
        return line;
    }

    public void AddToLogFile(string message)
    {
        if (!string.IsNullOrEmpty(LogFileName))
        {
            File.AppendAllText($"logs/debug_{LogFileName}.log", $"R {DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}", Encoding.UTF8);
        }
    }

}


using System.Text;
namespace lib.debug;
public class InterceptWriter : TextWriter
{
    private readonly TextWriter originalWriter;

    public InterceptWriter(TextWriter originalWriter)
    {
        this.originalWriter = originalWriter;
    }

    public override Encoding Encoding => originalWriter.Encoding;
#nullable enable
    public override void WriteLine(string? value)
    {
        originalWriter.WriteLine(value);
    }

    public override void Write(string? value)
    {
        originalWriter.Write(value);
    }
}
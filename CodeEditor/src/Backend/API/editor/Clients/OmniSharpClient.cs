using System.Text;
using System.Text.Json;
using System.Diagnostics;

public class OmniSharpClient
{
    private Process _process;
    private StreamWriter _input;
    private StreamReader _output;

    public void StartServer(string pathToOmniSharpExe)
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "mono",
                Arguments = pathToOmniSharpExe,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _process.Start();
        _input = _process.StandardInput;
        _output = _process.StandardOutput;
    }

    public void SendCompletionRequest(string filePath, int line, int character)
    {
        var request = new
        {
            Method = "textDocument/completion",
            Params = new
            {
                TextDocument = new { Uri = "file://" + filePath },
                Position = new { Line = line, Character = character }
            }
        };
        var json = JsonSerializer.Serialize(request);
        _input.WriteLine($"Content-Length: {json.Length}\r\n\r\n{json}");
        _input.Flush();
    }

    public string ReadResponse()
    {
        return _output.ReadLine(); // Simplified, you’d parse LSP JSON here
    }
}

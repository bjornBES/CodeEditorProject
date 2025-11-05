using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using Avalonia.Input;
using AvaloniaEdit.Editing;
using Avalonia.Media;
using AvaloniaEdit.Document;

public class OmniSharpCompletionProvider
{
    private readonly TextEditor _editor;
    private Process _omnisharpProcess;
    private StreamWriter _stdin;
    private StreamReader _stdout;
    private CompletionWindow? _completionWindow;

    public OmniSharpCompletionProvider(TextEditor editor)
    {
        _editor = editor;
        _editor.TextArea.TextEntered += OnTextEntered;
        _editor.TextArea.TextEntering += OnTextEntering;
    }

    public void StartOmniSharp(string pathToOmniSharpExe, string projectPath)
    {
        if (_omnisharpProcess != null && !_omnisharpProcess.HasExited)
            return;

        var psi = new ProcessStartInfo
        {
            FileName = "mono",
            Arguments = $"\"{pathToOmniSharpExe}\" -s \"{projectPath}\" --stdio",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _omnisharpProcess = Process.Start(psi)!;
        _stdin = _omnisharpProcess.StandardInput;
        _stdout = _omnisharpProcess.StandardOutput;

        Task.Run(ListenForResponses);
    }

    private async Task ListenForResponses()
    {
        while (!_stdout.EndOfStream)
        {
            var line = await _stdout.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("Content-Length"))
                continue; // skip headers

            try
            {
                using var doc = JsonDocument.Parse(line);
                if (doc.RootElement.TryGetProperty("method", out var method))
                {
                    if (method.GetString() == "textDocument/publishDiagnostics")
                        continue;
                }

                if (doc.RootElement.TryGetProperty("result", out var result))
                {
                    if (result.TryGetProperty("items", out var items))
                        ShowCompletions(items);
                }
            }
            catch
            {
                // Ignore partial JSON lines
            }
        }
    }

    private void OnTextEntered(object? sender, TextInputEventArgs e)
    {
        if (e.Text == ".")
        {
            RequestCompletion();
        }
    }

    private void OnTextEntering(object? sender, TextInputEventArgs e)
    {
        if (_completionWindow != null && e.Text.Length > 0 && !char.IsLetterOrDigit(e.Text[0]))
            _completionWindow.CompletionList.RequestInsertion(e);
    }

    private void RequestCompletion()
    {
        if (_omnisharpProcess == null || _omnisharpProcess.HasExited)
            return;

        var caret = _editor.TextArea.Caret;
        var request = new
        {
            Method = "textDocument/completion",
            Params = new
            {
                TextDocument = new { Uri = "file://" + "TempFile.cs" },
                Position = new { Line = caret.Line - 1, Character = caret.Column - 1 }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var msg = $"Content-Length: {Encoding.UTF8.GetByteCount(json)}\r\n\r\n{json}";
        _stdin.Write(msg);
        _stdin.Flush();
    }

    private void ShowCompletions(JsonElement items)
    {
        if (_completionWindow != null)
            _completionWindow.Close();

        _completionWindow = new CompletionWindow(_editor.TextArea);
        var data = _completionWindow.CompletionList.CompletionData;

        foreach (var item in items.EnumerateArray())
        {
            var label = item.GetProperty("label").GetString();
            if (!string.IsNullOrEmpty(label))
                data.Add(new SimpleCompletionData(label));
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => _completionWindow = null;
    }

    private class SimpleCompletionData : ICompletionData
    {
        public SimpleCompletionData(string text)
        {
            Text = text;
        }

        public string Text { get; }
        public object Content => Text;
        public object Description => Text;
        public double Priority => 0;

        IImage ICompletionData.Image => null;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}

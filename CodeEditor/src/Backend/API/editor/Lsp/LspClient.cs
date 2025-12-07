using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class LspClient : ILanguageClient
{
    private readonly Process lspProcess;
    private StreamWriter writer;
    private StreamReader reader;

    private Task listenerTask;


    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> pending = new ConcurrentDictionary<int, TaskCompletionSource<JsonElement>>();
    private readonly ConcurrentDictionary<string, int> fileVersions = new ConcurrentDictionary<string, int>();

    private readonly HashSet<string> openFiles = new();

    private int requestId = 1;
    private bool disposed = false;

    public string LanguageId { get; }
    public bool IsRunning => !lspProcess.HasExited;

    public LspClient(string languageId, string lspServer = "clangd")
    {
        LanguageId = languageId;

        ProcessStartInfo psi = new()
        {
            FileName = lspServer,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        lspProcess = new Process { StartInfo = psi };
        lspProcess.Start();
        writer = lspProcess.StandardInput;
        reader = lspProcess.StandardOutput;
        listenerTask = Task.Run(() => ReadLoopAsync());
    }

    public async Task InitializeAsync(string uri)
    {
        Uri root = new Uri(uri);
        var initParams = new
        {
            processId = Environment.ProcessId,
            rootUri = root.AbsoluteUri,
            capabilities = new { },
            workspaceFolders = new[]
            {
                new { uri = root.AbsoluteUri, name = Path.GetFileName(root.AbsolutePath) }
            }
        };

        await SendRequestAsync("initialize", initParams);
        await SendNotificationAsync("initialized", new { });

        Console.WriteLine($"[Info] LSP ({LanguageId}) initialized.");
    }

    public async Task DidOpenAsync(string uri, string text)
    {
        fileVersions[uri] = 1;

        await SendNotificationAsync("textDocument/didOpen", new
        {
            textDocument = new
            {
                uri,
                languageId = LanguageId,
                version = 1,
                text
            }
        });

        // Default cursor to end of file
        Console.WriteLine($"[Info] Opened {uri} ({LanguageId})");
    }

    public async Task DidChangeAsync(string uri, string updatedText)
    {
        if (!fileVersions.TryGetValue(uri, out int version))
            version = 1;

        version++;
        fileVersions[uri] = version;

        await SendNotificationAsync("textDocument/didChange", new
        {
            textDocument = new { uri, version },
            contentChanges = new[]
            {
                new { text = updatedText }
            }
        });


        Console.WriteLine($"[Info] Changed {uri} → v{version} ({LanguageId})");
    }

    public async Task DidCloseAsync(string uri)
    {
        fileVersions.TryRemove(uri, out _);

        await SendNotificationAsync("textDocument/didClose", new
        {
            textDocument = new { uri }
        });

        Console.WriteLine($"[Info] Closed {uri} ({LanguageId})");
    }

    public async Task DidSaveAsync(string uri)
    {
        await SendNotificationAsync("textDocument/didSave", new
        {
            textDocument = new { uri }
        });
    }

    public Task<JsonElement> CompletionAsync(string uri, int line, int character)
    {
        return SendRequestAsync("textDocument/completion", new
        {
            textDocument = new { uri },
            position = new { line, character },
            context = new { triggerKind = 1 }
        });
    }

    public Task<JsonElement> HoverAsync(string uri, int line, int character)
    {
        return SendRequestAsync("textDocument/hover", new
        {
            textDocument = new { uri },
            position = new { line, character }
        });
    }

    private async Task SendNotificationAsync(string method, object parameters)
    {
        if (disposed) return;

        var json = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method,
            @params = parameters
        });

        await SendRawAsync(json);
    }

    private async Task<JsonElement> SendRequestAsync(string method, object parameters)
    {
        if (disposed) throw new ObjectDisposedException(nameof(LspClient));

        int id = requestId++;
        var tcs = new TaskCompletionSource<JsonElement>();
        pending[id] = tcs;

        var json = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id,
            method,
            @params = parameters
        });

        await SendRawAsync(json);

        return await tcs.Task;
    }

    private async Task SendRawAsync(string json)
    {
        if (disposed) return;

        int length = Encoding.UTF8.GetByteCount(json);
        string header = $"Content-Length: {length}\r\n\r\n";

        await writer.WriteAsync(header + json);
        await writer.FlushAsync();
    }

    private async Task ReadLoopAsync()
    {
        try
        {
            while (!reader.EndOfStream)
            {
                string header = await reader.ReadLineAsync();
                if (header == null) break;

                if (!header.StartsWith("Content-Length:"))
                    continue;

                int contentLength = int.Parse(header.Substring("Content-Length:".Length).Trim());

                await reader.ReadLineAsync(); // blank line

                byte[] buffer = new byte[contentLength];
                int read = await lspProcess.StandardOutput.BaseStream.ReadAsync(buffer, 0, contentLength);

                string json = Encoding.UTF8.GetString(buffer, 0, read);
                HandleIncomingMessage(json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LSP ERROR] Read loop error: {ex}");
        }
    }

    private void HandleIncomingMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Push notifications (diagnostics, log messages, etc.)
            if (root.TryGetProperty("method", out var methodProp))
            {
                string method = methodProp.GetString();

                if (method == "textDocument/publishDiagnostics")
                {
                    HandleDiagnostics(root.GetProperty("params"));
                    return;
                }

                Console.WriteLine($"[LSP] Notification: {method}");
                return;
            }

            // Responses to requests
            if (root.TryGetProperty("id", out var idProp))
            {
                int id = idProp.GetInt32();
                if (pending.TryRemove(id, out var tcs))
                {
                    if (root.TryGetProperty("result", out var result))
                        tcs.TrySetResult(result);
                    else if (root.TryGetProperty("error", out var err))
                        tcs.TrySetException(new Exception(err.ToString()));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LSP ERROR] JSON parse error: {ex}");
        }
    }

    private void HandleDiagnostics(JsonElement parms)
    {
        string uri = parms.GetProperty("uri").GetString();
        var diagnostics = parms.GetProperty("diagnostics");

        Console.WriteLine($"\nDiagnostics for {Path.GetFileName(uri)}:");
        foreach (var diag in diagnostics.EnumerateArray())
        {
            string msg = diag.GetProperty("message").GetString();
            int line = diag.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32();
            int col = diag.GetProperty("range").GetProperty("start").GetProperty("character").GetInt32();

            Console.WriteLine($"  ({line + 1},{col + 1})  {msg}");
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        try
        {
            // graceful shutdown
            _ = SendRequestAsync("shutdown", new { }).Result;
        }
        catch { }

        try
        {
            _ = SendNotificationAsync("exit", new { }).Wait(100);
        }
        catch { }

        try { lspProcess.Kill(); } catch { }

        lspProcess.Dispose();
        Console.WriteLine("[LSP] Client disposed.");
    }
}

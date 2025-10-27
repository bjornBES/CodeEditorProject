
using System.Diagnostics;
using System.Runtime.InteropServices;

public class LanguageToolManager
{
    private static readonly HttpClient HttpClient = new HttpClient();
    /// <summary>
    /// Ensures Tree-sitter native library and an LSP server are available for a given language.
    /// Downloads prebuilt binaries if missing and launches the LSP process.
    /// </summary>
    /// <param name="languageId">Language identifier, e.g., "python", "javascript", "rust"</param>
    /// <returns>Running LSP process</returns>
    public static async Task<Process> EnsureLanguageToolsAsync(string languageId)
    {
        string os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : throw new PlatformNotSupportedException();

        string arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException()
        };

        // ----------------------------
        // Map languageId to LSP release URLs
        // ----------------------------
        Dictionary<string, string> languageServers = new Dictionary<string, string>
            {
                { "javascript", "https://registry.npmjs.org/typescript-language-server/-/typescript-language-server-0.13.0.tgz" },
                { "python", "https://registry.npmjs.org/pyright/-/pyright-1.1.301.tgz" },
                { "rust", "https://github.com/rust-analyzer/rust-analyzer/releases/latest/download/rust-analyzer-" + (os=="windows"?"x86_64-pc-windows-msvc.exe":os=="linux"?"x86_64-unknown-linux-gnu":"aarch64-apple-darwin") }
            };

        if (!languageServers.TryGetValue(languageId.ToLower(), out string lspUrl))
            throw new Exception($"No LSP configured for language '{languageId}'");

        string lspFolder = Path.Combine(AppContext.BaseDirectory, "lsp", languageId);
        Directory.CreateDirectory(lspFolder);

        string lspFileName = Path.GetFileName(lspUrl);
        string lspPath = Path.Combine(lspFolder, lspFileName);

        if (!File.Exists(lspPath))
        {
            Console.WriteLine($"Downloading LSP server for {languageId}...");
            byte[] bytes = await HttpClient.GetByteArrayAsync(lspUrl); // <-- use the static HttpClient
            await File.WriteAllBytesAsync(lspPath, bytes);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("chmod", $"+x {lspPath}")?.WaitForExit();
            }

            Console.WriteLine($"LSP server downloaded to {lspPath}");
        }

        // ----------------------------
        // Launch LSP
        // ----------------------------
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = lspPath,
            Arguments = "--stdio",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = new Process { StartInfo = psi };
        process.Start();

        Console.WriteLine($"LSP server started: {process.Id} ({languageId})");
        return process;
    }

}
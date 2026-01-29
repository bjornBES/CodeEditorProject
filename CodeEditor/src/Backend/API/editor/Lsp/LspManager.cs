public static class LspManager
{
    private static readonly Dictionary<string, ILanguageClient> _clients = new();

    public static void Init()
    {
        var csharpClient = new LspClient("csharp", @"omnisharp.exe");
        _clients[".cs"] = csharpClient;

        var jsonClient = new LspClient("json", @"json-lsp.exe");
        _clients[".json"] = jsonClient;

    }

    public static ILanguageClient GetClientForFile(string filePath)
    {
        string ext = Path.GetExtension(filePath);
        if (_clients.TryGetValue(ext, out var client))
            return client;
        return null;
    }

    public static IEnumerable<ILanguageClient> GetAllClients() => _clients.Values;
}

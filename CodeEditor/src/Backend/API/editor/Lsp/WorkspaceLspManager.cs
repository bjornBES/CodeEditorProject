using System.Collections.Concurrent;

public class WorkspaceLspManager : IDisposable
{
    private readonly ConcurrentDictionary<string, LspClient> servers = new();

    public async Task AddServerAsync(string languageId, string workspaceDir, string lspServer)
    {
        if (servers.ContainsKey(languageId)) return;

        var client = new LspClient(languageId, lspServer);
        await client.InitializeAsync(new Uri(workspaceDir).AbsoluteUri);
        servers[languageId] = client;
    }

    public LspClient GetServer(string languageId)
    {
        servers.TryGetValue(languageId, out var client);
        return client;
    }

    public void Dispose()
    {
        foreach (var client in servers.Values)
            client.Dispose();
    }
}

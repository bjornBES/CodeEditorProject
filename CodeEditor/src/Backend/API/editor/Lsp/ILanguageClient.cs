

public interface ILanguageClient : IDisposable
{
    Task InitializeAsync(string rootUri);

    Task DidOpenAsync(string uri, string text);
    Task DidChangeAsync(string uri, string updatedText);
    Task DidCloseAsync(string uri);
    Task DidSaveAsync(string uri);

/*
    Task<HoverResult?> HoverAsync(string uri, Position position);
    Task<CompletionResult?> CompletionAsync(string uri, Position position);
    Task<DefinitionResult?> DefinitionAsync(string uri, Position position);
    Task<FormattingResult?> FormatAsync(string uri);

    event Action<string uri, IReadOnlyList<Diagnostic>> DiagnosticsReceived;
*/

    // Communication status
    bool IsRunning { get; }
}

public class DocumentLspSession
{
    public ILanguageClient LanguageClient { get; }
    public FileEditorInput Document { get; }

    public DocumentLspSession(FileEditorInput document, ILanguageClient languageClient)
    {
        LanguageClient = languageClient;
        Document = document;
    }

    public async Task OpenAsync()
    {
        await LanguageClient.DidOpenAsync(
            Document.FilePath,
            Document.TextContent
        );
    }

    public async Task DidChangeAsync(string newText)
    {
        await LanguageClient.DidChangeAsync(
            Document.FilePath,
            newText
        );
    }

    public async Task DidSaveAsync()
    {
        await LanguageClient.DidSaveAsync(Document.FilePath);
    }
}
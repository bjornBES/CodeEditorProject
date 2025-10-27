
using AvaloniaEdit.Document;
using AvaloniaEdit.Indentation;

public static class IndentationManager
{
    private static readonly Dictionary<string, IIndentationProvider> providers = new();

    public static void RegisterProvider(IIndentationProvider provider)
        => providers[provider.LanguageId] = provider;

    public static void IndentLine(string languageId, TextDocument document, DocumentLine line, int tabSize, bool useTabs)
    {
        if (providers.TryGetValue(languageId, out IIndentationProvider provider))
            provider.IndentLine(document, line, tabSize, useTabs);
        else
            new DefaultIndentationStrategy().IndentLine(document, line);
    }
    public static void IndentAfterEnter(string languageId, TextDocument document, int lineNumber, int tabSize, bool useTabs)
    {
        if (lineNumber <= 0 || lineNumber > document.LineCount)
            return;

        DocumentLine line = document.GetLineByNumber(lineNumber);

        IndentLine(languageId, document, line, tabSize, useTabs);
    }
}
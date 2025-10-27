
using AvaloniaEdit.Document;

public interface IIndentationProvider
{
    public string LanguageId { get; }
    public void IndentLine(TextDocument document, DocumentLine line, int tabSize, bool useTabs);

}

using AvaloniaEdit.Document;

public interface IIndentationStrategy_BES
{
    public void IndentLine(TextDocument document, DocumentLine line);
    public void IndentLines(TextDocument document, int beginLine, int endLine);
}
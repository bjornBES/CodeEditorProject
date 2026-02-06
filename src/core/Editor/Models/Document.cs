using shared;

namespace TextEditorProject.Core.Editor.Models
{
    public sealed class Document
    {
        public DocumentId Id { get; }
        public string? FilePath { get; private set; }

        public TextBuffer Buffer { get; }
        public long Version { get; private set; }

        public Document(DocumentId id, TextBuffer buffer, string? filePath = null)
        {
            Id = id;
            Buffer = buffer;
            FilePath = filePath;
            Version = 0;
        }

        public void Insert(Position pos, string text)
        {
            Buffer.Insert(pos, text);
            Version++;
        }

        public bool IsEmpty()
        {
            return Buffer.IsEmpty;
        }
    }

}
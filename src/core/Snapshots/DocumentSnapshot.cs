using TextEditorProject.Core.Editor.Models;

namespace TextEditorProject.Core.Snapshots
{
    public sealed class DocumentSnapshot
    {
        public DocumentId Id { get; }
        public string? FilePath { get; }
        public long Version { get; }

        public IReadOnlyList<LineSnapshot> Lines { get; }

        private DocumentSnapshot(DocumentId id, string? filePath, long version, IReadOnlyList<LineSnapshot> lines)
        {
            Id = id;
            FilePath = filePath;
            Version = version;
            Lines = lines;
        }

        public static DocumentSnapshot From(Document doc)
        {
            var lines = doc.Buffer.GetAllLines().Select((text, index) => new LineSnapshot(index, text)).ToList();

            return new DocumentSnapshot(
                doc.Id,
                doc.FilePath,
                doc.Version,
                lines
            );
        }
    }

}
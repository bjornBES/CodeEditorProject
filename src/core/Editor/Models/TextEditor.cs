
namespace TextEditorProject.Core.Editor.Models
{
    public sealed class TextEditor
    {
        public EditorId Id { get; }
        public Document Document { get; }

        public CursorState Cursors { get; }
        public SelectionState Selection { get; }

        // Extensions live here later (LSP, diagnostics, etc)

        public TextEditor(EditorId id, Document document)
        {
            Id = id;
            Document = document;
            Cursors = new CursorState();
            Selection = new SelectionState();
        }
    }
}

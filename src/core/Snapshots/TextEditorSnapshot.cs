using TextEditorProject.Core.Contracts;
using TextEditorProject.Core.Editor.Models;

namespace TextEditorProject.Core.Snapshots
{
    public sealed class TextEditorSnapshot
    {
        public EditorId Id { get; }
        public DocumentId DocumentId { get; }

        public CursorSnapshot Cursor { get; }
        public SelectionSnapshot? Selection { get; }

        private TextEditorSnapshot(EditorId id, DocumentId documentId, CursorSnapshot cursor, SelectionSnapshot? selection)
        {
            Id = id;
            DocumentId = documentId;
            Cursor = cursor;
            Selection = selection;
        }

        public static TextEditorSnapshot From(TextEditor editor)
        {
            return new TextEditorSnapshot(editor.Id, editor.Document.Id, CursorSnapshot.From(editor.Cursors), null/* selection later */);
        }
    }

}
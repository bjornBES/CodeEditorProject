using TextEditorProject.Core.Engine;
using TextEditorProject.Core.Editor.Models;
using TextEditorProject.Core.Editor;

#nullable disable

namespace TextEditorProject.Core.Snapshots
{
    public struct EditorSnapshot
    {
        public int Version { get; }
        public IReadOnlyList<DocumentSnapshot> Documents { get; }
        public IReadOnlyList<TextEditorSnapshot> Editors { get; }
        public EditorId ActiveEditorId { get; }

#nullable restore
        public TextEditorSnapshot? CurrentActiveEditor { get => GetTextEditorSnapshot(ActiveEditorId); }
        public DocumentSnapshot? CurrentActiveDocument { get => GetDocumentSnapshot(CurrentActiveEditor?.DocumentId); }
#nullable disable

        // public IReadOnlyList<DiagnosticSnapshot> Diagnostics { get; init; }

        public EditorSnapshot(EditorState state)
        {
            if (state.EditorWorkspace != null)
            {
                EditorWorkspace workspace = state.EditorWorkspace;
                Documents = workspace.Documents.Values.Select(DocumentSnapshot.From).ToList();
                Editors = workspace.Editors.Values.Select(TextEditorSnapshot.From).ToList();
                ActiveEditorId = workspace.ActiveEditorId;
            }
        }

        public static EditorSnapshot FromState(EditorState state)
        {
            return new EditorSnapshot(state);
        }
#nullable restore
        public TextEditorSnapshot? GetTextEditorSnapshot(EditorId id)
        {
            TextEditorSnapshot? editorSnapshot = null;
            foreach (var editor in Editors)
            {
                if (editor.Id.Value == id.Value)
                {
                    editorSnapshot = editor;
                    break;
                }
            }
            if (editorSnapshot == null)
            {
                Console.WriteLine($"Could not find editor with ID {id}");
            }
            return editorSnapshot;
        }
        public DocumentSnapshot? GetDocumentSnapshot(DocumentId? id)
        {
            if (!id.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "null is not an valid ID");
            }
            DocumentSnapshot? documentSnapshot = null;
            foreach (var document in Documents)
            {
                if (document.Id.Value == id.Value.Value)
                {
                    documentSnapshot = document;
                    break;
                }
            }
            if (documentSnapshot == null)
            {
                Console.WriteLine($"Could not find document with ID {id}");
            }
            return documentSnapshot;
        }
    }

    public class DiagnosticSnapshot
    {
    }

}
#nullable restore
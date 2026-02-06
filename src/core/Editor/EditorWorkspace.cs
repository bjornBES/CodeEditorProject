using System.Net.NetworkInformation;
using shared;
using TextEditorProject.Core.Commands;
using TextEditorProject.Core.Editor.Models;

namespace TextEditorProject.Core.Editor
{
    public enum CursorMovementTo
    {
        left, right, up, down,
    }
    public enum CursorMovementBy
    {
        line, character
    }
    public sealed class EditorWorkspace
    {
        private readonly Dictionary<DocumentId, Document> _documents = new();
        private readonly Dictionary<EditorId, TextEditor> _editors = new();

        public IReadOnlyDictionary<DocumentId, Document> Documents => _documents;
        public IReadOnlyDictionary<EditorId, TextEditor> Editors => _editors;

        public EditorId ActiveEditorId { get; internal set; }
        private IEditorEngine Engine;

        public bool HasActiveEditor => ActiveEditorId.Value != Guid.Empty;


        // ---------- creation / destruction ----------

        public EditorWorkspace(IEditorEngine engine)
        {
            Engine = engine;
            ActiveEditorId = new EditorId(Guid.Empty);
            engine.Registry.Register(CommandHelper.Create("editor.newEditor", (ctx) =>
            {
                TextBuffer buffer = new TextBuffer("");
                Document document = CreateDocument(buffer, "");
                TextEditor textEditor = CreateEditor(document.Id);
                return Task.CompletedTask;
            }));
            engine.Registry.Register(CommandHelper.Create<string>("editor.openFile", (ctx, file) =>
            {
                openFile(ctx, file);
                return Task.CompletedTask;
            }));
            engine.Registry.Register(CommandHelper.Create<Position, string>("editor.insertAt", (ctx, position, src) =>
            {
                if (!HasActiveEditor)
                {
                    engine.Execute("editor.newEditor");
                }

                TextEditor editor = Editors[ActiveEditorId];
                if (position.Line > editor.Document.Buffer.LineCount)
                {
                    Console.WriteLine($"Status: could not insert line at {position} in editor {editor.Id}");
                    return Task.CompletedTask;
                }

                editor.Document.Insert(position, src);
                return Task.CompletedTask;
            }));
            engine.Registry.Register(CommandHelper.Create<EditorId, Position, string>("editor.insert.EditorAt", (ctx, id, position, src) =>
            {
                if (!Editors.ContainsKey(id))
                {
                    Console.WriteLine($"Status: could not insert line in editor {id}");
                    return Task.CompletedTask;
                }

                ActiveEditorId = id;

                engine.Execute("editor.insertAt", position, src);
                return Task.CompletedTask;
            }));
            engine.Registry.Register(CommandHelper.Create("cursor.left", (ctx) =>
            {
                ctx.Commands.Execute("cursor.move", CursorMovementTo.left, CursorMovementBy.character, 1);
                return Task.CompletedTask;
            }));
            engine.Registry.Register(CommandHelper.Create("cursor.right", (ctx) =>
            {
                ctx.Commands.Execute("cursor.move", CursorMovementTo.right, CursorMovementBy.character, 1);
                return Task.CompletedTask;
            }));
            engine.Registry.Register(CommandHelper.Create("cursor.up", (ctx) =>
            {
                ctx.Commands.Execute("cursor.move", CursorMovementTo.up, CursorMovementBy.character, 1);
                return Task.CompletedTask;
            }));
            engine.Registry.Register(CommandHelper.Create("cursor.down", (ctx) =>
            {
                ctx.Commands.Execute("cursor.move", CursorMovementTo.down, CursorMovementBy.character, 1);
                return Task.CompletedTask;
            }));
            engine.Registry.Register(CommandHelper.Create<CursorMovementTo, CursorMovementBy, int>("cursor.move", (ctx, to, by, value) =>
            {
                CursorState cursor = GetEditor(ActiveEditorId).Cursors;
                Position posValue;
                switch (to)
                {
                    case CursorMovementTo.up:
                        posValue = new Position(-1, 0);
                        break;
                    case CursorMovementTo.down:
                        posValue = new Position(1, 0);
                        break;
                    case CursorMovementTo.left:
                        posValue = new Position(0, -1);
                        break;
                    case CursorMovementTo.right:
                        posValue = new Position(0, 1);
                        break;
                    default:
                        return Task.CompletedTask;
                }

                Document document = GetEditor(ActiveEditorId).Document;
                int deltaLine = Math.Max(posValue.Line + cursor.Primary.Line, 0);
                deltaLine = Math.Min(deltaLine, document.Buffer.LineCount - 1);
                int deltaColumn = Math.Max(posValue.Column + cursor.Primary.Column, 0);
                Position delta = new Position(deltaLine, deltaColumn);
                cursor.MoveTo(delta);
                return Task.CompletedTask;
            }));
        }

        private void openFile(CommandContext ctx, string path)
        {
            // is the file open already?
            foreach (TextEditor editor in _editors.Values)
            {
                Document document = editor.Document;
                if (Path.Exists(document.FilePath) &&
                Path.GetFullPath(document.FilePath) == path)
                {
                    ActiveEditorId = editor.Id;
                    return;
                }
            }

            string fileText = File.ReadAllText(path);
            // if any editors are open
            if (!HasActiveEditor)
            {
                TextBuffer buffer = new TextBuffer(fileText);
                Document document = CreateDocument(buffer, path);
                CreateEditor(document.Id);
                return;
            }
            else
            {
                TextEditor editor = _editors[ActiveEditorId];
                Document document = editor.Document;
                if (document.IsEmpty())
                {
                    document = new Document(document.Id, document.Buffer, path);
                    document.Insert(new Position(0, 0), fileText);
                }
            }
            return;
        }

        public Document CreateDocument(TextBuffer buffer, string? filePath = null)
        {
            var doc = new Document(DocumentId.New(), buffer, filePath);
            _documents.Add(doc.Id, doc);
            return doc;
        }

        public TextEditor CreateEditor(DocumentId documentId)
        {
            var doc = _documents[documentId];
            var editor = new TextEditor(EditorId.New(), doc);
            _editors.Add(editor.Id, editor);
            ActiveEditorId = editor.Id;
            return editor;
        }

        public void CloseEditor(EditorId editorId)
        {
            _editors.Remove(editorId);
        }

        public void CloseDocument(DocumentId documentId)
        {
            // close all editors on this doc
            foreach (var editor in _editors.Values.Where(e => e.Document.Id == documentId).ToList())
                _editors.Remove(editor.Id);

            _documents.Remove(documentId);
        }

        // ---------- command helpers ----------

        public TextEditor GetEditor(EditorId id) => _editors[id];
        public Document GetDocument(DocumentId id) => _documents[id];
    }
}

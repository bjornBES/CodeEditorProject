using TextEditorProject.Core.Snapshots;
using TextEditorProject.Rendering.UI.Abstractions;

namespace TextEditorProject.Rendering.UI
{
    public sealed class UiLayout : IUiProvider
    {
        public UiNode Create()
        {
            return new UiColumnNode("root").Also((root) =>
            {
                root.Add(
                [
                    /*
                    new UiRowNode("test").Also((rowTest) =>
                    {
                        rowTest.Add(
                        [
                            new UiButtonNode("open"),
                            new UiButtonNode("save"),
                        ]);
                    }),
                    */

                    new UiEditorNode("main-editor")
                ]);
            });
        }

        public void Update(UiNode root, EditorSnapshot snapshot)
        {
            TextEditorSnapshot? editor = snapshot.CurrentActiveEditor;
            var saveButton = root.Find<UiButtonNode>("save");
            // saveButton.IsEnabled = snapshot.IsDirty;

            if (editor != null)
            {
                var editorNode = root.Find<UiEditorNode>("main-editor");
                editorNode.EditorId = editor.Id;
                editorNode.DocumentId = editor.DocumentId;
            }
        }
    }
}
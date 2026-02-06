using TextEditorProject.Core.Snapshots;
using TextEditorProject.Rendering.UI.Abstractions;

namespace TextEditorProject.Rendering.UI
{
    public interface IUiProvider
    {
        UiNode Create();
        void Update(UiNode root, EditorSnapshot snapshot);
    }
}
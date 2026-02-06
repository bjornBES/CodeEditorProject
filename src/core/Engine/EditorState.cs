using TextEditorProject.Core.Editor;

namespace TextEditorProject.Core.Engine
{
    public sealed class EditorState
    {
        public EditorWorkspace EditorWorkspace;
    
        public EditorState(IEditorEngine engine)
        {
            EditorWorkspace = new EditorWorkspace(engine);
        }
    }
}
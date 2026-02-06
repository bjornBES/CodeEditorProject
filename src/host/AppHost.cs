using TextEditorProject.Core.Snapshots;
using TextEditorProject.Rendering;
using TextEditorProject.Rendering.UI;
using TextEditorProject.Rendering.UI.Abstractions;

namespace TextEditorProject.Host
{
    class AppHost
    {
        private readonly IEditorEngine _engine;
        private readonly IRenderBackend _renderer;

        public AppHost(IEditorEngine engine, IRenderBackend renderer)
        {
            _engine = engine;
            _renderer = renderer;
            
            _renderer.CreateWindow(engine, "Text editor", 800, 600);
        }

        public void Run()
        {
            UiLayout layout = new UiLayout();
            UiNode node = layout.Create();
            _renderer.RootNote = node;
            // Main loop (UI framework usually handles this)
            // Could be event-driven
            while (_renderer.IsRunning)
            {
                EditorSnapshot? snapshot = _engine.Snapshot();
                _renderer.Run();
                if (snapshot.HasValue)
                {
                    layout.Update(node, snapshot.Value);
                }
                _renderer.Render(snapshot);
            }

            _renderer.Close();
        }
    }
}
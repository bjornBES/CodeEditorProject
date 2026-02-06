using TextEditorProject.Core.Contracts;
using TextEditorProject.Core.Snapshots;
using TextEditorProject.Rendering.UI.Abstractions;

namespace TextEditorProject.Rendering
{
    public interface IRenderBackend
    {
        bool IsRunning { set; get; }
        UiNode RootNote { get; set; }
        /// <summary>
        /// Creates a new window with a title and size.
        /// </summary>
        void CreateWindow(IEditorEngine engine, string title, int width, int height);

        /// <summary>
        /// Adds an interactive element (button, menu, etc.).
        /// </summary>
        // void AddElement(object element); // object for generic, can create IUIElement later

        /// <summary>
        /// Renders a full snapshot of the editor.
        /// </summary>
        void Render(EditorSnapshot? snapshot);

        /// <summary>
        /// Handles the main loop or pumping events.
        /// </summary>
        void Run();

        /// <summary>
        /// Shuts down the backend cleanly.
        /// </summary>
        void Close();
    }

}
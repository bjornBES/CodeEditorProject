using Avalonia;
using TextEditorProject.Core.Snapshots;
using TextEditorProject.Rendering;
using TextEditorProject.Rendering.UI.Abstractions;

namespace TextEditorProject.Rendering.backend.Avalonia
{
    public class AvaloniaRenderBackend : IRenderBackend
    {
        string[] Args;
        public AvaloniaRenderBackend(string[] args)
        {
            Args = args;
        }
        public bool IsRunning { get; set; }
        public UiNode RootNote { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
#nullable disable
        AvaloniaApp Application;
        #nullable restore
        public void Close()
        {
            
        }

        public void CreateWindow(IEditorEngine engine, string title, int width, int height)
        {
            Application = new AvaloniaApp();
            Application.AppTitle = title;
            Application.Width = width;
            Application.Height = height;
            Application.Engine = engine;
            _ = BuildAvaloniaApp().StartWithClassicDesktopLifetime(Args);
        }

        public void Render(EditorSnapshot? snapshot)
        {
            if (snapshot.HasValue)
            {
                Application.UpdateWindow(snapshot.Value);
            }
        }

        public void Run()
        {
        }

        AppBuilder BuildAvaloniaApp() => AppBuilder.Configure(() => Application).UsePlatformDetect().LogToTrace();
    }
}
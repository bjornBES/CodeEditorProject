using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using TextEditorProject.Core.Snapshots;

namespace TextEditorProject.Rendering.backend.Avalonia
{
    internal class AvaloniaApp : Application
    {
        #nullable disable
        public MainWindow MainWindow;
        public string AppTitle { get; internal set; }
        public IEditorEngine Engine { get; internal set; }
#nullable restore
        public int Width { get; internal set; }
        public int Height { get; internal set; }

        public void UpdateWindow(EditorSnapshot editorSnapshot)
        {
            if (MainWindow != null)
            {
                MainWindow.Update(editorSnapshot);
            }
        }

        public override void Initialize()
        {
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow = new MainWindow(Engine, AppTitle, Width, Height);
                desktop.MainWindow = MainWindow;

                // desktop.MainWindow.AttachDevTools();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
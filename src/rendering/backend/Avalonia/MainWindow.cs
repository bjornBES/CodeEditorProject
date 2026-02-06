using System.ComponentModel;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using NativeFileDialogSharp;
using ReactiveUI;
using TextEditorProject.Core.Input;
using TextEditorProject.Core.Snapshots;

namespace TextEditorProject.Rendering.backend.Avalonia
{
    #nullable disable
    public class MainWindow : Window
    {
        public IEditorEngine Engine;
        public MainWindow(IEditorEngine engine, string title, int width, int height)
        {
            // Width = width;
            // Height = height;
            // Title = title;

            Engine = engine;
            InitializeComponent();
            // KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //Engine.InputAdapter.KeyDown(new InputEvent(e.PhysicalKey))
            
            Console.WriteLine($"phys: {e.PhysicalKey}, key: {e.Key}, symbol: {e.KeySymbol}");

            e.Handled = true;
        }

        public void InitializeComponent()
        {
        }

        public void Update(EditorSnapshot editorSnapshot)
        {
            
        }

    }
}
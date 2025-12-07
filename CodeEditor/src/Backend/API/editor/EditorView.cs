
using Avalonia.Controls;

public class EditorView : Panel
{
    // public EditorTabControl EditorTabControl { get; set; }
    public BackendEditor BackendEditor{ get; set; }

    public EditorView()
    {
        BackendEditor = new BackendEditor();
        // EditorTabControl = new EditorTabControl(BackendEditor.EditorService.ActiveGroup, BackendEditor.registryOptions);
    }

    public void OpenFile(string path, string title)
    {
        EditorBuffer buffer = BackendEditor.NewTextBuffer();
        BackendEditor.OpenFile(path, title, buffer);
        // EditorTabControl.UpdateTabs();
    }
}
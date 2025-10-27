
/// <summary>
/// An implementation of EditorInput that represents a file on disk.
/// </summary>
public class FileEditorInput : EditorInput
{
    public string FilePath { get; }
    public string TextContent { get; private set; }

    public FileEditorInput(string path)
    {
        FilePath = path;
        Title = Path.GetFileName(path);
        Resource = new Uri(path, UriKind.RelativeOrAbsolute);
        TextContent = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    public override void Save()
    {
        File.WriteAllText(FilePath, TextContent);
        IsDirty = false;
    }

    public override void Revert()
    {
        Console.WriteLine($"Reverting {FilePath}");
        IsDirty = false;
    }

    public override void Dispose()
    {
        Console.WriteLine($"Disposing {FilePath}");
    }

    public void UpdateContent(string text)
    {
        TextContent = text;
        IsDirty = true;
    }
}

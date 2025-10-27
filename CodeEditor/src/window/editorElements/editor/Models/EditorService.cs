
using Avalonia.Controls;

public class EditorService
{
    private readonly List<EditorGroup> _groups = new();
    public IReadOnlyList<EditorGroup> Groups => _groups;
    public EditorGroup ActiveGroup { get; private set; }
    public TextBlock fileInfoTextBlock { get; private set; }

    public EditorService()
    {
        fileInfoTextBlock = new TextBlock();
        EditorGroup rootGroup = new EditorGroup();
        _groups.Add(rootGroup);
        ActiveGroup = rootGroup;
        
    }

    public void OpenEditor(EditorInput input, object view, bool pinned = false)
    {
        ActiveGroup.OpenEditor(input, view, pinned);
    }

    public void CloseEditor(EditorInput input)
    {
        ActiveGroup.CloseEditor(input);
    }
}

public class EditorTab
{
    public EditorInput Input { get; }
    public object View { get; set; }
    public bool IsActive { get; set; }
    public bool IsPinned { get; set; }
    public bool _isFocused { get; set; }
    public bool IsFocused
    {
        get
        {
            return _isFocused;
        }
        set
        {
            _isFocused = value;
        }
    }
    public bool IsPreview { get; set; }

    public EditorTab(EditorInput input)
    {
        Input = input;
    }
}

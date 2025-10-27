/// Represents something that can be opened in an editor tab
public abstract class EditorInput
{
    public string Title { get; protected set; }
    public Uri Resource { get; protected set; }
    public bool IsDirty { get; protected set; }

    public abstract void Save();
    public virtual void Revert() {}
    public virtual void Dispose() {}
}

namespace TextEditorProject.Core.Editor.Models
{
    public readonly record struct EditorId(Guid Value)
    {
        public static EditorId New() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
    }
}

namespace TextEditorProject.Rendering.UI
{
    public abstract record UiAction;

    public record InvokeCommand(string CommandId, IReadOnlyList<object>? Args)
        : UiAction;

    public record MoveCaret(int LineDelta, int ColumnDelta)
        : UiAction;

    public record InsertText(string Text)
        : UiAction;

    public record DeleteSelection() : UiAction;
}
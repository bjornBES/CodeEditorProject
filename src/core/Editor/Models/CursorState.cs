using shared;

namespace TextEditorProject.Core.Editor.Models
{
    public sealed class CursorState
    {
        public Position Primary { get; private set; } = new(0, 0);

        public void MoveTo(Position pos)
        {
            Primary = pos;
        }

        public void MoveAfterInsert(string text)
        {
            if (!text.Contains('\n'))
            {
                Primary = new Position(Primary.Line, Primary.Column + text.Length);
                return;
            }

            var parts = text.Split('\n');
            Primary = new Position(
                Primary.Line + parts.Length - 1,
                parts[^1].Length
            );
        }
    }
}

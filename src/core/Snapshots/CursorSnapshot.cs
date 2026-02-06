using shared;
using TextEditorProject.Core.Editor.Models;

namespace TextEditorProject.Core.Snapshots
{
    public readonly record struct CursorSnapshot(Position Position)
    {
        public static CursorSnapshot From(CursorState state)
            => new(state.Primary);
    }
}

using shared;

namespace TextEditorProject.Core.Editor.Models
{
    public sealed class TextBuffer
    {
        private readonly List<string> _lines = new();

        public int LineCount => _lines.Count;

        public TextBuffer(string text = "")
        {
            _lines.AddRange(text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'));
        }

        public string GetLine(int index) => _lines[index];

        public IReadOnlyList<string> GetAllLines() => _lines;

        public bool IsEmpty => _lines.Any((line) => line.Length == 0);

        public void Insert(Position pos, string text)
        {
            var line = _lines[pos.Line];
            var before = line[..pos.Column];
            var after = line[pos.Column..];

            var parts = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

            if (parts.Length == 1)
            {
                _lines[pos.Line] = before + parts[0] + after;
                return;
            }

            _lines[pos.Line] = before + parts[0];

            for (int i = 1; i < parts.Length - 1; i++)
            {
                _lines.Insert(pos.Line + i, parts[i]);
            }

            _lines.Insert(pos.Line + parts.Length - 1, parts[^1] + after);
        }
    }
}

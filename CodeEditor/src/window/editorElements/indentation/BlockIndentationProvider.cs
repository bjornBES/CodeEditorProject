using AvaloniaEdit.Document;
using AvaloniaEdit.Indentation;
using TextMateSharp.Grammars;
using System.Linq;

public class BlockIndentationProvider : IIndentationProvider
{
    public string LanguageId { get; }

    private readonly IGrammar grammar;

    public BlockIndentationProvider(string languageId, IGrammar grammar)
    {
        LanguageId = languageId;
        this.grammar = grammar;
    }

    public void IndentLine(TextDocument document, DocumentLine line, int tabSize, bool useTabs)
    {
        if (line.IsDeleted) return;

        string lineText = document.GetText(line);
        int currentLineNumber = line.LineNumber;

        if (currentLineNumber <= 1)
        {
            ReplaceIndent(document, line, "");
            return;
        }

        int indentLevel = GetIndentLevel(document, currentLineNumber);

        string trimmed = lineText.TrimStart();
        int desiredLevel = trimmed.StartsWith("}") ? Math.Max(0, indentLevel - 1) : indentLevel;

        string newIndent = BuildIndentString(desiredLevel, tabSize, useTabs);
        ReplaceIndent(document, line, newIndent);
    }

    private int GetIndentLevel(TextDocument document, int lineNumber)
    {
        int level = 0;

        for (int ln = 1; ln < lineNumber; ln++)
        {
            DocumentLine docLine = document.GetLineByNumber(ln);
            string text = document.GetText(docLine);
            if (string.IsNullOrEmpty(text)) continue;

            ITokenizeLineResult tokenized = grammar.TokenizeLine(text);
            IToken[] tokens = tokenized.Tokens;

            if (tokens.Length > 0)
            {
                level = CountBracesFromTokens(text, tokens, level);
            }
            else
            {
                level = CountBracesNaive(text, level);
            }
        }

        return level;
    }

    private int CountBracesFromTokens(string text, IToken[] tokens, int currentLevel)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            IToken token = tokens[i];
            int start = token.StartIndex;
            int end = (i == tokens.Length - 1) ? text.Length : tokens[i + 1].StartIndex;

            if (IsStringOrCommentToken(token)) continue;

            for (int p = start; p < end; p++)
            {
                char c = text[p];
                if (c == '{') currentLevel++;
                else if (c == '}') currentLevel = Math.Max(0, currentLevel - 1);
            }
        }

        return currentLevel;
    }

    private int CountBracesNaive(string text, int currentLevel)
    {
        bool inString = false;

        for (int p = 0; p < text.Length; p++)
        {
            char c = text[p];
            if (c == '"' || c == '\'') inString = !inString;
            if (inString) continue;

            if (c == '{') currentLevel++;
            else if (c == '}') currentLevel = Math.Max(0, currentLevel - 1);
        }

        return currentLevel;
    }

    private bool IsStringOrCommentToken(IToken token)
    {
        IList<string> scopes = token.Scopes;
        for (int i = 0; i < scopes.Count; i++)
        {
            string scope = scopes[i];
            if (scope.IndexOf("string", StringComparison.OrdinalIgnoreCase) >= 0 ||
                scope.IndexOf("comment", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }
        return false;
    }

    private void ReplaceIndent(TextDocument document, DocumentLine line, string newIndent)
    {
        string text = document.GetText(line);
        int oldIndentLength = text.Length - text.TrimStart().Length;
        document.Replace(line.Offset, oldIndentLength, newIndent);
    }

    private string BuildIndentString(int level, int tabSize, bool useTabs)
    {
        if (useTabs)
        {
            string indent = "";
            for (int i = 0; i < level; i++) indent += "\t";
            return indent;
        }
        else
        {
            return new string(' ', level * tabSize);
        }
    }


}

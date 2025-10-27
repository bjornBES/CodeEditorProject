
public class EditorSection
{
    public int FontSize { get; set; }
    public int FontWeight { get; set; }
    public string FontFamily { get; set; }

    public AutoIndent AutoIndent { get; set; }
    public string IndentSize { get; set; }
    public bool InsertSpaces { get; set; }
    public int IndentWidth { get; set; }

    public bool WordWrap { get; set; }
    // and so on for all the settings that effect the editor
}

public enum AutoIndent
{
    None = 0,
    Full = 1,
}
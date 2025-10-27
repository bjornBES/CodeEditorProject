
using System.Text.Json.Serialization;

public class Theme
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } // "dark" or "light"
    [JsonPropertyName("colors")]
    public Dictionary<string, ThemeElement> Colors { get; set; }
    [JsonPropertyName("semanticHighlighting")]
    public bool SemanticHighlighting { get; set; }
    [JsonPropertyName("semanticTokenColors")]
    public Dictionary<string, SemanticStyle> SemanticTokenColors { get; set; } // can refine later
}

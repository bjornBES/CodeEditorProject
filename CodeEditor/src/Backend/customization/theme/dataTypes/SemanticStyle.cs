using System.Text.Json.Serialization;

public class SemanticStyle
{
    [JsonPropertyName("foreground")]
    public string Foreground { get; set; }
    [JsonPropertyName("background")]
    public string Background { get; set; }
    [JsonPropertyName("fontStyle")]
    public FontStyleOptions FontStyle { get; set; }
}

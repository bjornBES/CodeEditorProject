using System.Formats.Asn1;
using System.Text.Json.Serialization;
public class ThemeElement
{
#nullable enable
    [JsonPropertyName("foreground")]
    public string? Foreground { get; set; }
    [JsonPropertyName("background")]
    public string? Background { get; set; }

    [JsonPropertyName("events")]
    public Dictionary<string, ThemeEventElement>? Events { get; set; }

    [JsonPropertyName("subElements")]
    public Dictionary<string, ThemeElement>? SubElements { get; set; }
}

public class ThemeEventElement
{
    [JsonPropertyName("background")]
    public string? Background { get; set; }
    [JsonPropertyName("foreground")]
    public string? Foreground { get; set; }
    [JsonPropertyName("fontStyle")]
    public FontStyleOptions? FontStyleOptions { get; set; }
}
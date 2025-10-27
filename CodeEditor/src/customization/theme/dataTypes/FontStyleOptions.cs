using System.Text.Json.Serialization;

public class FontStyleOptions
{
    [JsonPropertyName("bold")]
    public bool Bold { get; set; }
    [JsonPropertyName("italic")]
    public bool Italic { get; set; }
    [JsonPropertyName("underline")]
    public bool Underline { get; set; }
    [JsonPropertyName("strikethrough")]
    public bool Strikethrough { get; set; }
}

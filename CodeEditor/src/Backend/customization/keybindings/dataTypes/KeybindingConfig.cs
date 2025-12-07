
using System.Text.Json.Serialization;

public class KeybindingConfig
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("command")]
    public string CommandId { get; set; }

#nullable enable
    [JsonPropertyName("when")]
    public string? Context { get; set; } = "global";
}
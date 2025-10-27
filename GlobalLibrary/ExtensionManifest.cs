
using System.Text.Json.Serialization;

namespace GlobalLibrary;

#nullable enable

public class ExtensionManifest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("displayname")]
    public string DisplayName { get; set; } = "";
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";
    [JsonPropertyName("order")]
    public int? Order { get; set; }
    [JsonPropertyName("main")]
    public string? Main { get; set; }
    [JsonPropertyName("entry")]
    public string? Entry { get; set; }
    [JsonPropertyName("contributes")]
    public ExtensionContributes? Contributes;
}

public class ExtensionContributes
{
    [JsonPropertyName("commands")]
    public ExtensionContributesCommands[]? Commands { get; set; }
}

public class ExtensionContributesCommands
{
    [JsonPropertyName("command")]
    public string Command { get; set; } = "";
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
}
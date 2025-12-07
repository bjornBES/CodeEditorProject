
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Avalonia;

public class EditorConfigs : Settings<EditorConfigs>
{
    public EditorConfigs()
    {
        Editor = new EditorSection();
        Window = new WindowSection();
    }
    [JsonPropertyName("editor")]
    public EditorSection Editor { get; set; }
    [JsonPropertyName("window")]
    public WindowSection Window { get; set; }

    // Free-form extension configs
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extensions { get; set; } = new();

    // Helper to read dynamic values from extensions
    public T Get<T>(string section, string key, T defaultValue = default)
    {
        if (Extensions.TryGetValue(section, out JsonElement sectionData) &&
            sectionData.ValueKind == JsonValueKind.Object &&
            sectionData.TryGetProperty(key, out JsonElement element))
        {
            try
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText()) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public void Set<T>(string extensionName, string key, T value)
    {
        // Ensure extension section exists
        if (!Extensions.TryGetValue(extensionName, out JsonElement sectionElement) ||
            sectionElement.ValueKind != JsonValueKind.Object)
        {
            Extensions[extensionName] = JsonSerializer.SerializeToElement(new Dictionary<string, object>());
            sectionElement = Extensions[extensionName];
        }

        // Convert to JsonObject so we can update it
        JsonObject obj = JsonNode.Parse(sectionElement.GetRawText())!.AsObject();

        obj[key] = JsonSerializer.SerializeToNode(value);

        // Save back
        Extensions[extensionName] = JsonSerializer.SerializeToElement(obj);
    }
    public bool Remove(string extensionName, string key)
    {
        if (!Extensions.TryGetValue(extensionName, out JsonElement sectionElement) ||
            sectionElement.ValueKind != JsonValueKind.Object)
            return false;

        JsonObject obj = JsonNode.Parse(sectionElement.GetRawText())!.AsObject();

        if (obj.Remove(key))
        {
            Extensions[extensionName] = JsonSerializer.SerializeToElement(obj);
            return true;
        }

        return false;
    }

    // Remove the entire extension section
    public bool RemoveExtension(string extensionName)
    {
        return Extensions.Remove(extensionName);
    }

    // Add a new extension section (optionally with initial key/values)
    public void AddExtension(string extensionName, Dictionary<string, object> initialData = null)
    {
        if (Extensions.ContainsKey(extensionName))
            return; // already exists

        JsonElement obj = initialData != null
            ? JsonSerializer.SerializeToElement(initialData)
            : JsonSerializer.SerializeToElement(new Dictionary<string, object>());

        Extensions[extensionName] = obj;
    }

    public override void MergeSettings(EditorConfigs otherConfig)
    {
        // 1. Merge editor (typed)
        if (otherConfig.Editor != null)
        {
            if (otherConfig.Editor.FontSize != default)
                Editor.FontSize = otherConfig.Editor.FontSize;

            if (otherConfig.Editor.FontWeight != default)
                Editor.FontWeight = otherConfig.Editor.FontWeight;

            if (!string.IsNullOrWhiteSpace(otherConfig.Editor.FontFamily))
                Editor.FontFamily = otherConfig.Editor.FontFamily;

            if (otherConfig.Editor.AutoIndent != default)
                Editor.AutoIndent = otherConfig.Editor.AutoIndent;

            if (!string.IsNullOrWhiteSpace(otherConfig.Editor.IndentSize))
                Editor.IndentSize = otherConfig.Editor.IndentSize;

            if (otherConfig.Editor.InsertSpaces != default)
                Editor.InsertSpaces = otherConfig.Editor.InsertSpaces;

            if (otherConfig.Editor.IndentWidth != default)
                Editor.IndentWidth = otherConfig.Editor.IndentWidth;

            if (otherConfig.Editor.WordWrap != default)
                Editor.WordWrap = otherConfig.Editor.WordWrap;

            if (otherConfig.Window.FontScale != 1f)
                Window.FontScale = otherConfig.Window.FontScale;
        }

        // 2. Merge extensions (free-form JSON dictionaries)
        foreach (KeyValuePair<string, JsonElement> kvp in otherConfig.Extensions)
        {
            if (Extensions.TryGetValue(kvp.Key, out JsonElement existingSection) &&
                    existingSection.ValueKind == JsonValueKind.Object &&
                    kvp.Value.ValueKind == JsonValueKind.Object)
            {
                // Deep merge keys
                JsonObject baseObj = JsonNode.Parse(existingSection.GetRawText())!.AsObject();
                JsonObject otherObj = JsonNode.Parse(kvp.Value.GetRawText())!.AsObject();
                foreach (KeyValuePair<string, JsonNode> prop in otherObj)
                {
                    baseObj[prop.Key] = prop.Value;
                }
                Extensions[kvp.Key] = JsonSerializer.SerializeToElement(baseObj);
            }
            else
            {
                // Just add or overwrite
                Extensions[kvp.Key] = kvp.Value;
            }
        }
    }
    public override void ChangedFile(EditorConfigs newConfig)
    {
        MergeSettings(newConfig);
        Application.Current.Resources["allFontScale"] = newConfig.Window.FontScale;
    }
}


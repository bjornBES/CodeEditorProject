using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;

public class ThemeService
{
    private static Theme _currentTheme;
    public static Theme CurrentTheme => _currentTheme;

    // Define only atomic states
    public static readonly string[] EventStates =
        ["default", "hover", "clicked", "focus", "disabled", "selected"];

    // Priority order for merging (like CSS specificity)
    private static readonly string[] MergePriority =
        ["disabled", "focus", "selected", "hover", "clicked"];

    // Store parsed theme values before merging
    private static readonly Dictionary<string, Dictionary<string, Dictionary<string, Color>>> _themeResources = new();

    public static Styles ThemeStyles { get; } = new Styles();

    public static void SetTheme(string name)
    {
        string path = Path.Combine(AppPaths.ThemesDirectoryPath, $"{name}.json");
        string json = File.ReadAllText(path);

        _currentTheme = JsonSerializer.Deserialize<Theme>(json);
        ApplyUITheme(_currentTheme);
    }

    public static void SetRegistryOptions(Editor editor)
    {
        RegistryOptions registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        // foreach (var panel in editor.editorPanels)
        // {
        //     panel.ApplyRegistryOptions(registryOptions);
        // }
    }

    private static void ApplyUITheme(Theme theme)
    {
        if (Application.Current == null) return;
        _themeResources.Clear();

        if (theme.Colors != null)
        {
            foreach (KeyValuePair<string, ThemeElement> kv in theme.Colors)
            {
                ApplyThemeElement(kv.Key, kv.Value, kv.Key);
            }
        }
    }

    private static void ApplyThemeElement(string key, ThemeElement element, string prefix)
    {
        if (!_themeResources.ContainsKey(prefix))
            _themeResources[prefix] = new();

        // Default state
        if (!_themeResources[prefix].ContainsKey("default"))
            _themeResources[prefix]["default"] = new();

        if (!string.IsNullOrEmpty(element.Background))
        {
            Color color = ConvertHex(element.Background);
            _themeResources[prefix]["default"]["background"] = color;
            addResources(Application.Current.Resources, $"{prefix}.background", color);
        }

        if (!string.IsNullOrEmpty(element.Foreground))
        {
            Color color = ConvertHex(element.Foreground);
            _themeResources[prefix]["default"]["foreground"] = color;
            addResources(Application.Current.Resources, $"{prefix}.foreground", color);
        }

        // Event states
        if (element.Events != null)
        {
            foreach (KeyValuePair<string, ThemeEventElement> ev in element.Events)
            {
                string state = ev.Key;
                ThemeEventElement eventElement = ev.Value;

                if (!_themeResources[prefix].ContainsKey(state))
                    _themeResources[prefix][state] = new();

                if (!string.IsNullOrEmpty(eventElement.Background))
                {
                    Color color = ConvertHex(eventElement.Background);
                    _themeResources[prefix][state]["background"] = color;
                    addResources(Application.Current.Resources, $"{prefix}.{state}.background", color);
                }

                if (!string.IsNullOrEmpty(eventElement.Foreground))
                {
                    Color color = ConvertHex(eventElement.Foreground);
                    _themeResources[prefix][state]["foreground"] = color;
                    addResources(Application.Current.Resources, $"{prefix}.{state}.foreground", color);
                }
            }
        }

        // Sub elements
        if (element.SubElements != null)
        {
            foreach (KeyValuePair<string, ThemeElement> sub in element.SubElements)
            {
                ApplyThemeElement(sub.Key, sub.Value, $"{prefix}.{sub.Key}");
            }
        }
    }

    /// <summary>
    /// Get merged colors for a component given active states.
    /// </summary>
    public static IDictionary<string, Color> GetMergedState(string prefix, params string[] states)
    {
        Dictionary<string, Color> merged = new Dictionary<string, Color>();

        if (!_themeResources.ContainsKey(prefix))
            return merged;

        Dictionary<string, Dictionary<string, Color>> stateDict = _themeResources[prefix];

        foreach (string state in MergePriority)
        {
            if (states.Contains(state))
            {
                if (stateDict.TryGetValue(state, out Dictionary<string, Color> values))
                {
                    foreach (KeyValuePair<string, Color> kv in values)
                    {
                        merged[kv.Key] = kv.Value; // override lower priority
                    }
                }
            }
        }

        if (merged.Count == 0)
        {
            if (stateDict.TryGetValue("default", out Dictionary<string, Color> values))
            {
                foreach (KeyValuePair<string, Color> kv in values)
                {
                    merged[kv.Key] = kv.Value; // override lower priority
                }
            }
        }

        return merged;
    }

    private static void addResources(IResourceDictionary resources, string key, object value)
    {
        if (resources.ContainsKey(key))
        {
            resources[key] = value;
        }
        else
        {
            resources.Add(key, value);
        }
    }

    private static Color ConvertHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.Parse("#FF00FF");
        if (!hex.StartsWith("#")) hex = "#" + hex;
        return Color.Parse(hex);
    }
}

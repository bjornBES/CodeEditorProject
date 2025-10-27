
using System.Text.Json;
using Avalonia.Input;

public static class KeybindingLoader
{
    public static void LoadKeybindings()
    {
        if (!File.Exists(AppPaths.KeybindingsFilePath) || File.ReadAllText(AppPaths.KeybindingsFilePath).Trim().Length == 0)
        {
            return;
        }
        string json = File.ReadAllText(AppPaths.KeybindingsFilePath);
        List<KeybindingConfig> bindings = JsonSerializer.Deserialize<List<KeybindingConfig>>(json);

        foreach (KeybindingConfig binding in bindings)
        {
            if (TryParseKey(binding.Key, out Key key, out KeyModifiers modifiers))
            {
                KeybindingManager.BindKey(key, modifiers, binding.Context, binding.CommandId);
            }
        }
    }

    private static bool TryParseKey(string keyString, out Key key, out KeyModifiers modifiers)
    {
        KeyGesture gesture = KeyGesture.Parse(keyString);

        modifiers = gesture.KeyModifiers;
        key = gesture.Key;
        
        return key != Key.None;
    }

}
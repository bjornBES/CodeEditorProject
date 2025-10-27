
using System.Runtime.CompilerServices;
using System.Text.Json;

public class SettingsManager<T> where T : Settings<T>
{
    private readonly object _lock = new();
    public T Current { get; private set; }
    public string GlobalPath;
    public string WorkspacePath;

    public event Action OnConfigChanged;
    public SettingsManager(string path)
    {
        GlobalPath = path;
        CreateDefaultInstance();
    }

    public SettingsManager(string local, string global)
    {
        WorkspacePath = local;
        GlobalPath = global;
        CreateDefaultInstance();
    }

    private void CreateDefaultInstance()
    {
        Current = JsonSerializer.Deserialize<T>("{}");
    }



    public void Load()
    {
        T global = LoadFromFile(GlobalPath) ?? default;

        // Start with global
        Current = global;

        // If workspace is set, merge overrides
        if (!string.IsNullOrEmpty(WorkspacePath) && File.Exists(WorkspacePath))
        {
            T workspace = LoadFromFile(WorkspacePath);
            if (workspace is not null)
            {
                MergeSettings(global, workspace);
            }
        }
        else if (!string.IsNullOrEmpty(WorkspacePath))
        {
            // Create default workspace file if missing
            SaveToFile(WorkspacePath, default);
        }
        OnConfigChanged?.Invoke();
    }

    public void SaveGlobal()
    {
        SaveToFile(GlobalPath, Current);
    }

    public void SaveWorkspace()
    {
        if (!string.IsNullOrEmpty(WorkspacePath))
        {
            SaveToFile(WorkspacePath, Current);
        }
        else
        {
            SaveGlobal();
        }
    }

    private T LoadFromFile(string path)
    {
        if (!File.Exists(path)) return null;

        string contents = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(contents))
        {
            Current = JsonSerializer.Deserialize<T>("{}");
            contents = JsonSerializer.Serialize(Current, new JsonSerializerOptions() { WriteIndented = true });
        }

        try
        {
            return JsonSerializer.Deserialize<T>(contents);
        }
        catch (JsonException)
        {
            File.Move(path, path + ".bak", overwrite: true);
            return null;
        }
    }

    private void SaveToFile(string path, T configs)
    {
        lock (_lock)
        {
            if (!File.Exists(path))
            {
                AppPaths.EnsureDirectoriesExist();
                AppPaths.EnsureFilesExist();
            }
            string json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        OnConfigChanged?.Invoke();
    }

    /// <summary>
    /// Merge another config into the current one.
    /// Values in "other" override values in "baseConfig".
    /// </summary>
    public void MergeSettings(T baseConfig, T other)
    {
        baseConfig.MergeSettings(other);
    }

    public void ChangeLoaclPath(string path)
    {
        WorkspacePath = path;
    }

    public void RunOnConfigChanged()
    {
        OnConfigChanged?.Invoke();
    }
}
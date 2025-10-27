
public static class AppPaths
{
    #region AppDataPaths
    /// <summary>
    /// this is for the global config files under %%AppData%% or ~/.config
    /// </summary>
    public static readonly string AppDataDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Program.AppName);

    public static readonly string GlobalConfigFilePath = Path.Combine(AppDataDirectoryPath, "settings.json");

    public static readonly string KeybindingsFilePath = Path.Combine(AppDataDirectoryPath, "keybindings.json");

    public static readonly string KeyboardLayoutFilePath = Path.Combine(AppDataDirectoryPath, "keybindings.json");

    #region GlobalStoragePaths
    /// <summary>
    /// This is where the recent workspace/directory/files will be at
    /// </summary>
    public static readonly string GlobalStorageDirectoryPath = Path.Combine(AppDataDirectoryPath, "globalStorage");

    /// <summary>
    /// Contants recent projects, default theme and other things
    /// </summary>
    public static readonly string GlobalStorageFilePath = Path.Combine(GlobalStorageDirectoryPath, "storage.json");

    #endregion

    #endregion

    #region UserProfilePaths
    /// <summary>
    /// this is where the extensions and themes will be stored (for now)
    /// </summary>
    public static readonly string UserProfileDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{Program.AppName}");

    public static readonly string ExtensionsDirectoryPath = Path.Combine(UserProfileDirectoryPath, "extensions");

    public static readonly string ThemesDirectoryPath = Path.Combine(UserProfileDirectoryPath, "themes");
    #endregion

    #region WorkspacePaths
    /// <summary>
    /// This is where the ativce workspace will be at
    /// </summary>
    public static string WorkspaceDirectoryPath { get; private set; } = ""; //fallback/temp workspace directory
    public static string WorkspaceEditorDirectoryPath => !string.IsNullOrEmpty(WorkspaceDirectoryPath) ? Path.Combine(WorkspaceDirectoryPath, ".editor") : string.Empty;
    public static string WorkspaceConfigFilePath => !string.IsNullOrEmpty(WorkspaceDirectoryPath) ? Path.Combine(WorkspaceEditorDirectoryPath, "settings.json") : string.Empty;
    #endregion

    #region TempPaths

    public static readonly string TempDirectoryPath = Path.Combine(AppDataDirectoryPath, "Temp");

    public static readonly string TempCommandDirectoryPath = Path.Combine(TempDirectoryPath, "Commands");

    #endregion

    #region DownloadedAssets

    public static readonly string DownloadedAssetsDirectoryPath = Path.Combine(AppDataDirectoryPath, "Downloads");

    #endregion


    public static void EnsureDirectoriesExist()
    {
        ensureDirectoryExists(AppDataDirectoryPath);
        ensureDirectoryExists(GlobalStorageDirectoryPath);

        ensureDirectoryExists(UserProfileDirectoryPath);
        ensureDirectoryExists(ExtensionsDirectoryPath);
        ensureDirectoryExists(ThemesDirectoryPath);

        ensureDirectoryExists(DownloadedAssetsDirectoryPath);

        ensureDirectoryExists(TempDirectoryPath);
        ensureDirectoryExists(TempCommandDirectoryPath);

        if (!string.IsNullOrEmpty(WorkspaceDirectoryPath))
        {
            ensureDirectoryExists(WorkspaceEditorDirectoryPath);
        }
    }

    private static void ensureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static void EnsureFilesExist()
    {
        ensureFileExist(GlobalConfigFilePath);
        ensureFileExist(KeybindingsFilePath);
        ensureFileExist(GlobalStorageFilePath);

        KeybindingLoader.LoadKeybindings();

        if (!string.IsNullOrEmpty(WorkspaceDirectoryPath))
        {
            ensureFileExist(WorkspaceConfigFilePath);
        }
    }

    private static void ensureFileExist(string path)
    {
        if (!File.Exists(path))
        {
            using FileStream fs = File.Create(path);
        }
    }

    public static void SetWorkspacePath(string workspacePath)
    {
        WorkspaceDirectoryPath = workspacePath;
    }
}
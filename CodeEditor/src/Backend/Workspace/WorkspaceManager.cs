
using System.Text.Json;

public class WorkspaceManager
{

    public static string _workspacePath = AppPaths.ProjectLocationPath;

    // The currently opened workspace
    internal static Workspace currentWorkspace;
    /// <summary>
    /// Will open a workspace from a given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Workspace OpenWorkspace(string name)
    {
        string[] workspaces = Directory.GetDirectories(_workspacePath);

        foreach (string workspace in workspaces)
        {
            if (workspace.EndsWith(name, StringComparison.OrdinalIgnoreCase))
            {
                string workspacePath = Path.GetFullPath(workspace);
                if (!workspaceExists(workspacePath))
                {
                    break;
                }
                currentWorkspace = openWorkspace(workspacePath);
                return currentWorkspace;
            }
        }

        Console.WriteLine($"Could not find workspace {name}");
        return null;
    }

    /// <summary>
    /// Deserializes the workspace files and returns the workspace
    /// </summary>
    /// <param name="path">path to workspace</param>
    /// <returns>the workspace</returns>
    static Workspace openWorkspace(string path)
    {
        string workspaceFile = Path.Combine(path, "workspace.json");
        if (!workspaceExists(path))
        {
            Console.WriteLine($"Could not find the workspace file in {path}");
            return null;
        }

        string workspaceJsonContents = File.ReadAllText(workspaceFile);
        Workspace workspace = JsonSerializer.Deserialize<Workspace>(workspaceJsonContents);

        if (!Directory.Exists(workspace.Path))
        {
            Directory.CreateDirectory(workspace.Path);
        }

        // Deserialize other files in the workspace directory
        string[] files = Directory.GetFiles(path);
        foreach (string file in files)
        {
            if (file.Equals("workspace.json", StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }
            string filePath = Path.GetFullPath(file);
        }
        return workspace;
    }

    public static void SaveWorkspace(Workspace workspace)
    {
        string workspacePath = Path.Combine(_workspacePath, workspace.Name);
        if (!workspaceExists(workspacePath))
        {
            // workspace dose not exist

            // then make it
            Directory.CreateDirectory(workspacePath);
        }

        // workspace exists
        string workspaceJsonContents = JsonSerializer.Serialize(workspace, new JsonSerializerOptions() { WriteIndented = true });
        string workspaceFile = Path.Combine(workspacePath, "workspace.json");
        File.WriteAllText(workspaceFile, workspaceJsonContents);
    }

    static bool workspaceExists(string path)
    {
        string workspaceFile = Path.Combine(path, "workspace.json");
        bool result = Directory.Exists(path) && File.Exists(workspaceFile);
        return result;
    }

    public static bool WorkspaceExists(string name)
    {
        string workspacePath = Path.Combine(_workspacePath, name);
        if (!workspaceExists(workspacePath))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Will get all documents from the currently opened workspace
    /// </summary>
    /// <returns></returns>
    public static Document[] GetDocuments(string searchPattern = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return GetDocuments(currentWorkspace, searchPattern, searchOption);
    }

    /// <summary>
    /// Will get all documents from a workspace
    /// </summary>
    /// <param name="workspace">the workspace</param>
    /// <returns></returns>
    public static Document[] GetDocuments(Workspace workspace, string searchPattern = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (workspace == null)
        {
            return null;
        }
        // TODO check if workspace.Path is an absolute path
        List<Document> documents = new List<Document>();
        string path = workspace.Path;
        string[] files = Directory.GetFiles(path, searchPattern, searchOption);
        foreach (string file in files)
        {
            string filePath = Path.Combine(path, file);
            documents.Add(new Document(filePath));
        }
        return documents.ToArray();
    }

    /// <summary>
    /// Will get the local config file from the workspace
    /// </summary>
    /// <returns></returns>
    public static object GetWorkspaceConfig()
    {
        return GetWorkspaceConfig(currentWorkspace);
    }

    /// <summary>
    /// Will get the local config file from the workspace
    /// </summary>
    /// <param name="workspace"></param>
    /// <returns></returns>
    public static object GetWorkspaceConfig(Workspace workspace)
    {
        string configFile = Path.Combine(workspace.Path, ".test", "config.json");
        // TODO: add the config class
        return null; // placeholder as per ^
    }
}
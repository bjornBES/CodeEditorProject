
using System.Runtime.InteropServices;

public static class DocumentManager
{
    /// <summary>
    /// Will open a document from the currently opened workspace
    /// </summary>
    /// <param name="path">relative path to file</param>
    public static Document OpenDocument(string path)
    {
        return OpenDocument(path, WorkspaceManager.currentWorkspace);
    }

    /// <summary>
    /// Will open a document from a workspace
    /// </summary>
    /// <param name="path">relative path to file</param>
    public static Document OpenDocument(string path, Workspace workspace)
    {
        string workspacePath = workspace.Path;
        string absFilePath;
        if (Path.IsPathRooted(path) && path.checkStartingPath(workspacePath))
        {
            absFilePath = path;
        }
        else
        {
            absFilePath = Path.Combine(workspacePath, path);
        }

        return new Document(absFilePath);
    }

    public static bool CreateNewDocument(string path)
    {
        return CreateNewDocument(path, WorkspaceManager.currentWorkspace);
    }

    public static bool CreateNewDocument(string path, Workspace workspace)
    {
        string workspacePath = workspace.Path;
        string absFilePath;
        if (Path.IsPathRooted(path) && path.checkStartingPath(workspacePath))
        {
            absFilePath = path;
        }
        else
        {
            absFilePath = Path.Combine(workspacePath, path);
        }

        try
        {
            File.WriteAllText(absFilePath, "HW");
            bool result = File.ReadAllText(absFilePath) == "HW";
            File.WriteAllText(absFilePath, "");
            return result;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    public static void CloseDocument(string path, Workspace workspace)
    {
        
    }

    static bool checkStartingPath(this string path1, string path2)
    {
        OperatingSystem operatingSystem = Environment.OSVersion;
        if (operatingSystem.Platform == PlatformID.Unix)
        {
            return path1.StartsWith(path2);
        }
        // windows needs to be 100% the same
        return path1.StartsWith(path2, StringComparison.CurrentCultureIgnoreCase);
    }
}
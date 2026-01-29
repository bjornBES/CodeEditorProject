
using System.ComponentModel;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using NativeFileDialogSharp;
using ReactiveUI;
using Tmds.DBus.Protocol;
using lib.debug;

public partial class MainWindow : Window
{
    string OpenFileDialog()
    {
        string path = OpenDialog(DialogType.OpenFile);
        if (path == null)
        {
            return null;
        }
        OpenFile(path);
        return path;
    }
    string OpenFile(string path)
    {
        if (GlobalStorageSettingsManager.Current.RecentFiles.Contains(path))
        {
            GlobalStorageSettingsManager.Current.RecentFiles.Remove(path);
        }
        GlobalStorageSettingsManager.Current.RecentFiles.Add(path);
        Document document = DocumentManager.OpenDocument(path);
        CodeEditor.OpenFile(document);
        GlobalStorageSettingsManager.SaveGlobal();
        return path;
    }
    void OpenFolder(string path)
    {
        if (string.IsNullOrEmpty(AppPaths.WorkspaceDirectoryPath))
        {
            EditorConfigsSettingsManager.SaveWorkspace();
        }
        if (!Directory.Exists(path))
        {
            DebugWriter.WriteLine("Main.API", $"directory dose not exist {path}");
            return;
        }
        Explorer.OpenFolder(path);
        CodeEditor.OnWorkspaceOpen();
        EditorConfigsSettingsManager.ChangeLoaclPath(AppPaths.WorkspaceConfigFilePath);
        EditorConfigsSettingsManager.Load();
        if (GlobalStorageSettingsManager.Current.RecentFolders.Contains(path))
        {
            GlobalStorageSettingsManager.Current.RecentFolders.Remove(path);
        }
        GlobalStorageSettingsManager.Current.RecentFolders.Add(path);
        GlobalStorageSettingsManager.SaveGlobal();
    }

    void SaveFile()
    {
        CodeEditor.SaveFile();
    }
    void SaveFileAs()
    {
        string path = OpenDialog(DialogType.OpenFile);
        if (path == null)
        {
            return;
        }
        CodeEditor.SaveFileAs(path);
    }
    void IndentDocument()
    {
        CodeEditor.IndentDocument();
    }

    void PinTab()
    {
        CodeEditor.PinTab();
    }
    void UnpinTab()
    {
        CodeEditor.UnpinTab();
    }
}
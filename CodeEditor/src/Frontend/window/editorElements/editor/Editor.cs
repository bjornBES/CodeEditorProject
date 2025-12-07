using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using lib.debug;
using TextMateSharp.Grammars;

public class Editor : Panel
{
    public BackendEditor BackendEditor;
    public EditorTabControl EditorTabControl { get; set; }

    // ---- LSP integration ----
    public WorkspaceLspManager LspManager { get; private set; } = new();

    public Editor()
    {
        BackendEditor = new BackendEditor();

        Background = Application.Current.Resources.GetResource("editor.background");
        IsVisible = true;
        EditorTabControl = new EditorTabControl(BackendEditor.EditorService.ActiveGroup, BackendEditor.registryOptions);
        Children.Add(EditorTabControl);

        CommandManager.RegisterCommand("Editor copy", "editor.copy", () => { });
        CommandManager.RegisterCommand("Editor paste", "editor.paste", () => { });
        CommandManager.RegisterCommand("Editor cut", "editor.cut", () => { });

        BackendEditor.UpdateTabs += EditorTabControl.UpdateTabs;
    }

    public void OnWorkspaceOpen()
    {
    }

    public async Task<LspClient?> GetOrStartServerAsync(string languageId)
    {
        var server = LspManager.GetServer(languageId);
        if (server != null)
            return server;

        string workspace = AppPaths.WorkspaceDirectoryPath;
        // Start server on-demand
        await LspManager.AddServerAsync(languageId, workspace, languageId switch
        {
            "c" => "clangd",
            "cpp" => "clangd",
            "python" => "pyright-langserver",
            _ => throw new NotSupportedException($"Language {languageId} not supported")
        });

        return LspManager.GetServer(languageId);
    }


    /// <summary>
    /// Opens a file in the editor.
    /// </summary>
    /// <param name="path">The path to the file</param>
    public void OpenFile(string path)
    {
        EditorBuffer buffer = BackendEditor.NewTextBuffer();
        buffer.View = new FileEditorView((FileEditorInput)buffer.Input);
        Document document = DocumentManager.OpenDocument(path);
        BackendEditor.OpenFile(document, buffer);
        buffer.View.UpdateText();
    }

    /// <summary>
    /// Updates the editor settings, such as font size and theme.
    /// </summary>
    public void UpdateSettings()
    {
        FileEditorView view = BackendEditor.EditorService.ActiveGroup?.ActiveTab?.View as FileEditorView;
        if (view == null)
        {
            return;
        }
        view.UpdateSettings();
    }

    /// <summary>
    /// Saves the current file.
    /// </summary>
    public void SaveFile()
    {
        if (GetActiveEditorView(out FileEditorView view))
        {
            EditorInput active = BackendEditor.EditorService.ActiveGroup?.ActiveTab?.Input;
            view.Save();
        }
    }

    /// <summary>
    /// Saves the current file as a new file.
    /// </summary>
    /// <param name="path">The path to the new file</param>
    public void SaveFileAs(string path)
    {
        DebugWriter.WriteLine("Editor", "Function Not Implemented SaveFileAs");
        EditorInput active = BackendEditor.EditorService.ActiveGroup?.ActiveTab?.Input;
        if (active is FileEditorInput fileInput)
        {
            File.WriteAllText(path, fileInput.TextContent);
            // TODO: Update the input to the new file path
        }
    }

    /// <summary>
    /// Closes the current tab.
    /// </summary>
    public void CloseTab()
    {
        EditorGroup group = BackendEditor.EditorService.ActiveGroup;
        if (group != null && group.ActiveTab != null)
        {
            group.CloseEditor(group.ActiveTab.Input);
            EditorTabControl.UpdateTabs();
        }
    }

    /// <summary>
    /// Creates a new tab with an empty file.
    /// </summary>
    public void NewTab()
    {
    }

    /// <summary>
    /// Formats the entire document.
    /// </summary>
    public void FormatDocument()
    {
    }

    /// <summary>
    /// Indents the entire document.
    /// </summary>
    public void IndentDocument()
    {
    }

    /// <summary>
    /// Pins the current tab so it won't be replaced by preview tabs.
    /// </summary>
    public void PinTab()
    {
        BackendEditor.PinTab();
    }
    /// <summary>
    /// Unpins the current tab.
    /// </summary>
    public void UnpinTab()
    {
        BackendEditor.UnpinTab();
    }

    /// <summary>
    /// Called when the config file changes.
    /// </summary>
    public void OnConfigChanged()
    {
        foreach (EditorGroup group in BackendEditor.EditorService.Groups)
        {
            foreach (EditorTab tab in group.Tabs)
            {
                if (tab.View is FileEditorView view)
                    view.OnConfigChanged();
            }
        }
    }

    /// <summary>
    /// Called to increase the editor font size.
    /// </summary>
    public void IncreaseEditorFontSize()
    {
        if (GetActiveEditorView(out FileEditorView view))
            view.Editor.FontSize++;

        int fontsize = (int)Application.Current.Resources["editor.fontsize"];
        fontsize += 1;
        Application.Current.Resources["editor.fontsize"] = fontsize;
        UpdateSettings();
    }
    /// <summary>
    /// Called to decrease the editor font size.
    /// </summary>
    public void DecreaseEditorFontSize()
    {
        if (GetActiveEditorView(out FileEditorView view))
            view.Editor.FontSize--;

        int fontsize = (int)Application.Current.Resources["editor.fontsize"];
        fontsize -= 1;
        if (fontsize <= 0) fontsize = 1;
        Application.Current.Resources["editor.fontsize"] = fontsize;
        UpdateSettings();
    }

    public void ApplyRegistryOptions(RegistryOptions newOptions)
    {
        BackendEditor.ApplyRegistryOptions(newOptions);
        EditorTabControl.UpdateTabs();
    }

    public bool GetActiveEditorView<T>(out T active) where T : class
    {
        return BackendEditor.GetActiveEditorView(out active);
    }

    public void OnClosed()
    {
        EditorTabControl.OnClosed();
    }
}
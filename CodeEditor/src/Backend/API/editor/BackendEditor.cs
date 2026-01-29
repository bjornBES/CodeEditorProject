using System.Diagnostics;
using Avalonia;
using TextMateSharp.Grammars;

public class BackendEditor
{
    int bufferCount = 0;
    public EditorService EditorService { get; set; }
    public RegistryOptions registryOptions { get; set; }

    // ---- LSP integration ----
    public WorkspaceLspManager LspManager { get; private set; } = new();

    public event Action UpdateTabs;
    public event Action CloseEditor;

    public BackendEditor()
    {
        registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        EditorService = new EditorService();
    }

    public void OnWorkspaceOpen()
    {
    }

    public async Task<LspClient> GetOrStartServerAsync(string languageId)
    {
        var server = LspManager.GetServer(languageId);
        if (server != null)
            return server;

        string workspace = WorkspaceManager.currentWorkspace.Path;
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
    public void OpenFile(string path, string title, EditorBuffer buffer)
    {
        buffer.Input.ChangePath(path, title);
        buffer.View.InitializeSyntaxHighlighting(registryOptions);
        EditorService.OpenEditor(buffer);
        UpdateTabs?.Invoke();
    }

    /// <summary>
    /// Opens a file in the editor.
    /// </summary>
    /// <param name="path">The path to the file</param>
    public void OpenFile(Document document, EditorBuffer buffer)
    {
        buffer.Input.ChangePath(document.URI.AbsolutePath, document.Name);
        buffer.View.InitializeSyntaxHighlighting(registryOptions);
        EditorService.OpenEditor(buffer);
        UpdateTabs?.Invoke();
    }

    /// <summary>
    /// Updates the editor settings, such as font size and theme.
    /// </summary>
    public void UpdateSettings()
    {
        EditorControlView view = EditorService.ActiveGroup?.ActiveTab?.View as EditorControlView;
        if (view == null)
        {
            return;
        }
        view.UpdateSettings();
    }

    /// <summary>
    /// Saves the current buffer into a file.
    /// </summary>
    public void SaveFile()
    {
        if (GetActiveEditorView(out EditorControlView view))
        {
            EditorInput active = EditorService.ActiveGroup?.ActiveTab?.Input;
            view.Input.Save();
        }
    }

    /// <summary>
    /// Saves the current file as a new file.
    /// </summary>
    /// <param name="path">The path to the new file</param>
    public void SaveFileAs(string path)
    {
        Console.WriteLine("Editor Function Not Implemented SaveFileAs");
        EditorInput active = EditorService.ActiveGroup?.ActiveTab?.Input;
        if (active is FileEditorInput fileInput)
        {
            File.WriteAllText(path, fileInput.TextContent);
            // TODO: Update the input to the new file path
        }
    }

    /// <summary>
    /// Closes the current buffer.
    /// </summary>
    public void CloseActiveBuffer()
    {
        EditorGroup group = EditorService.ActiveGroup;
        if (group != null && group.ActiveTab != null)
        {
            group.CloseEditor(group.ActiveTab.Input);
            UpdateTabs?.Invoke();
        }
    }

    /// <summary>
    /// Creates a new buffer.
    /// </summary>
    public EditorBuffer NewTextBuffer()
    {
        string tempPath = Path.GetTempFileName();
        FileEditorInput input = new FileEditorInput(tempPath, $"Buffer {bufferCount}");
        bufferCount++;
        EditorBuffer buffer = new EditorBuffer(input, null);
        return buffer;
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
        EditorTab tab = EditorService.ActiveGroup?.ActiveTab;
        if (tab != null)
            tab.IsPinned = true; // you’d extend EditorTab with IsPinned
    }
    /// <summary>
    /// Unpins the current tab.
    /// </summary>
    public void UnpinTab()
    {
        EditorTab tab = EditorService.ActiveGroup?.ActiveTab;
        if (tab != null)
            tab.IsPinned = false;
    }

    /// <summary>
    /// Called when the config file changes.
    /// </summary>
    public void OnConfigChanged()
    {
        foreach (EditorGroup group in EditorService.Groups)
        {
            foreach (EditorTab tab in group.Tabs)
            {
                if (tab.View is EditorControlView view)
                    view.OnConfigChanged();
            }
        }
    }

    public void ApplyRegistryOptions(RegistryOptions newOptions)
    {
        registryOptions = newOptions;

        foreach (EditorTab tab in EditorService.ActiveGroup.Tabs)
        {
            if (tab.View is EditorControlView view)
                view.ApplyTheme(newOptions);
        }
        UpdateTabs?.Invoke();
    }

    public T GetActiveEditorView<T>() where T : class
    {
        return EditorService.ActiveGroup?.ActiveTab?.View as T;
    }
    public bool GetActiveEditorView<T>(out T active) where T : class
    {
        active = EditorService.ActiveGroup?.ActiveTab?.View as T;
        return active != null;
    }

    public void OnClosed()
    {
        CloseEditor?.Invoke();
    }
}
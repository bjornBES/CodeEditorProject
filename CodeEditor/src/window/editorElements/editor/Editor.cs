using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using lib.debug;
using TextMateSharp.Grammars;

public class Editor : Panel
{
    public EditorService EditorService { get; set; }
    public EditorTabControl EditorTabControl { get; set; }
    public RegistryOptions registryOptions { get; set; }

    public Editor()
    {
        Background = Application.Current.Resources.GetResource("editor.background");
        registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        IsVisible = true;
        EditorService = new EditorService();
        EditorTabControl = new EditorTabControl(EditorService.ActiveGroup, registryOptions);
        Children.Add(EditorTabControl);

        CommandManager.RegisterCommand("Editor copy", "editor.copy", () => { });
        CommandManager.RegisterCommand("Editor paste", "editor.paste", () => { });
        CommandManager.RegisterCommand("Editor cut", "editor.cut", () => { });
    }

    /// <summary>
    /// Opens a file in the editor.
    /// </summary>
    /// <param name="path">The path to the file</param>
    public void OpenFile(string path)
    {
        FileEditorInput input = new FileEditorInput(path);
        FileEditorView view = new FileEditorView(input, registryOptions);
        EditorService.OpenEditor(input, view);
        EditorTabControl.UpdateTabs();
    }

    /// <summary>
    /// Updates the editor settings, such as font size and theme.
    /// </summary>
    public void UpdateSettings()
    {
        FileEditorView view = EditorService.ActiveGroup?.ActiveTab?.View as FileEditorView;
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
            EditorInput active = EditorService.ActiveGroup?.ActiveTab?.Input;
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
        EditorInput active = EditorService.ActiveGroup?.ActiveTab?.Input;
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
        EditorGroup group = EditorService.ActiveGroup;
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
        string tempPath = Path.GetTempFileName();
        FileEditorInput input = new FileEditorInput(tempPath);
        FileEditorView view = new FileEditorView(input, registryOptions);
        EditorService.OpenEditor(input, view);
        EditorTabControl.UpdateTabs();
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
        registryOptions = newOptions;

        foreach (EditorTab tab in EditorService.ActiveGroup.Tabs)
        {
            if (tab.View is FileEditorView view)
                view.ApplyTheme(newOptions);
        }
        EditorTabControl.UpdateTabs();
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
        EditorTabControl.OnClosed();
    }
}
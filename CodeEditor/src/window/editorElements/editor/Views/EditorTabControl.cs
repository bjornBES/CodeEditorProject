

using Avalonia;
using Avalonia.Controls;
using DynamicData.Kernel;
using TextMateSharp.Grammars;

public class EditorTabControl : Panel
{
    private readonly EditorGroup _group;
    private readonly TabControl _tabHost;
    private readonly RegistryOptions _registryOptions;
    public Button fileInfoTextBlock = null;

    public EditorTabControl(EditorGroup group, RegistryOptions registryOptions)
    {
        _registryOptions = registryOptions;
        _group = group;
        _tabHost = new TabControl();
        _tabHost.SelectionChanged += (s, e) =>
        {
            if (_tabHost.SelectedContent is FileEditorView)
            {
                FileEditorView editorView = _tabHost.SelectedContent as FileEditorView;
                editorView.Editor.Focus();
                editorView.Focus();
                CommandManager.UpdateCommandEntry("editor.copy", editorView.Editor.Copy);
                CommandManager.UpdateCommandEntry("editor.paste", editorView.Editor.Paste);
                CommandManager.UpdateCommandEntry("editor.cut", editorView.Editor.Cut);

                EditorInput editorInput = editorView.Input;
                if (GetEditorTab(editorInput))
                {
                    _group.OpenEditor(editorInput, editorView);
                    EditorTab editorTab = _group.ActiveTab;
                }
            }
        };
        Children.Add(_tabHost);
        UpdateTabs();
        fileInfoTextBlock = null;
    }

    public void UpdateTabs()
    {
        if (fileInfoTextBlock == null)
        {
            fileInfoTextBlock = new Button
            {
                Content = "Hello",
            };
            fileInfoTextBlock.Click += (s, e) =>
            {

            };
            CommandManager.ExecuteCommand("view.status.add.button", fileInfoTextBlock, Dock.Right);
        }
        int count = 0;
        foreach (object tab in _tabHost.Items)
        {
            if (tab is TabItem item && item.Content is FileEditorView view)
            {
                view.Editor.TextArea.OnCaretPositionChanged -= null;
                view.GotFocus -= null;
                view.LostFocus -= null;
            }
        }
        _tabHost.Items.Clear();
        TabItem[] tabItems = _group.Tabs.Select(t =>
        {
            string header = t.Input.Title + (t.Input.IsDirty ? "*" : "");
            Control content;

            if (t.View.GetType() == typeof(FileEditorView))
            {
                FileEditorView item = (FileEditorView)t.View;
                item.Editor.TextArea.OnCaretPositionChanged += (s, e) =>
                {
                    item.UpdateFileInfo(fileInfoTextBlock);
                };
                item.Editor.TextArea.GotFocus += (s, e) =>
                {
                    e.Handled = true;
                    t.IsFocused = true;
                };
                item.Editor.TextArea.LostFocus += (s, e) =>
                {
                    e.Handled = true;
                    t.IsFocused = false;
                };
                item.GotFocus += (s, e) =>
                {
                    e.Handled = true;
                    t.IsFocused = true;
                    item.UpdateFileInfo(fileInfoTextBlock);
                };
                item.LostFocus += (s, e) =>
                {
                    e.Handled = true;
                    t.IsFocused = false;
                };
                content = item;
            }
            else
            {
                content = new TextBlock { Text = $"Unsupported editor: {t.Input.Title}" };
            }

            return makeNewTabItem(header, content, count++);
        }).AsArray();
        foreach (TabItem tab in tabItems)
        {
            _tabHost.Items.Add(tab);
        }
    }



    private TabItem makeNewTabItem(string header, Control control, int count)
    {
        StackPanel tabHeader = new StackPanel();
        Button CloseButton = new Button()
        {
            Content = "X"
        };
        CloseButton.Click += (s, e) => { CommandManager.ExecuteCommand("editor.action.closeActiveEditor"); };
        TextBlock TitleBox = new TextBlock()
        {
            Text = header,
        };
        tabHeader.Children.Add(TitleBox);
        tabHeader.Children.Add(CloseButton);

        TabItem tabItem = new TabItem
        {
            Header = tabHeader,
            Content = control,
            FontSize = 16,
            Height = 10,
            BorderThickness = new Thickness(1, 2, 1, 0),
            BorderBrush = Application.Current.Resources.GetResource("editor.tabs.items.border.background"),
            CornerRadius = new CornerRadius(5, 5, 0, 0),
            Name = "tabItem" + count
        };

        tabItem.AddPseudoClassesBackground("editor.tabs.background", "editor.tabs.items.hover.background", "pointerover"); // adding hover effect
        tabItem.AddPseudoClassesBackground("editor.tabs.background", "editor.tabs.items.selected.background", "selected"); // adding selected effect

        return tabItem;
    }

    public void OnClosed()
    {
        foreach (object tab in _tabHost.Items)
        {
            if (tab is TabItem item && item.Content is FileEditorView view)
            {
                view.OnClosed();
            }
        }
    }

    bool GetEditorTab(EditorInput editorTab)
    {
        foreach (EditorTab tab in _group.Tabs)
        {
            if (tab.Input is FileEditorInput fileEditorInput)
            {
                if (fileEditorInput == editorTab)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

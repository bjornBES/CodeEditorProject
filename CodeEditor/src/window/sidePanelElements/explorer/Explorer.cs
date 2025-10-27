using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ReactiveUI;
using System.Reactive;
using lib.debug;
using System.Collections;

public class ExplorerNode
{
    public string Header { get; set; }
    public string Path { get; set; }
    public List<ExplorerNode> Children { get; set; } = new List<ExplorerNode>();
    public bool IsDirectory { get; set; }
}

public class Explorer : SidePanelElement
{
    public string WorkspacePath;

    ContextMenu treeItemMenu;

    TreeView treeView;
    ScrollViewer scrollViewer;

    StackPanel MainPanel;
    StackPanel FolderNotOpened;

    TextBlock textBlock;
    Button button;

    Border FileExplore;

    public Explorer()
    {
        Header = "Explorer";
        IconKey = "explorerIcon";
        InitializeComponent();
        Application.Current.Resources["TreeViewItemIndent"] = 2.0;
    }

    public override void EndInit()
    {
        UpdateTreeContents();
        base.EndInit();
    }

    public void InitializeComponent()
    {
        FolderNotOpened = new StackPanel() { IsVisible = false };

        MainPanel = new StackPanel { Orientation = Orientation.Vertical };
        textBlock = new TextBlock
        {
            Margin = new Thickness(0, 20, 0, 10),
            Text = "You have not yet opened a folder.",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 14,
        };
        MainPanel.Children.Add(textBlock);

        button = new Button
        {
            Name = "explorerButton",
            Margin = new Thickness(0, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            Content = "Open Folder",
            FontSize = 14,
        };
        button.Click += (s, e) => OpenFolder();
        MainPanel.Children.Add(button);

        FolderNotOpened.Children.Add(MainPanel);

        FileExplore = new Border() { IsVisible = false };

        this.Children.Add(FolderNotOpened);
        this.Children.Add(FileExplore);

        treeView = new TreeView
        {
            IsVisible = true,
            SelectionMode = SelectionMode.Single
        };

        scrollViewer = new ScrollViewer();

        // SelectionChanged: use args.AddedItems to be safe
        treeView.SelectionChanged += (s, e) =>
        {
            IList added = e.AddedItems;
            if (added == null) return;

            // if added is a TreeViewItem (we construct TreeViewItem), read Tag
            if (added is TreeViewItem tvItem && tvItem.Tag is ExplorerNode nodeFromTag)
            {
                HandleNodeSelected(nodeFromTag, tvItem);
                e.Handled = true;
                return;
            }

            // if added is ExplorerNode (data model), use it directly
            if (added is ExplorerNode node)
            {
                HandleNodeSelected(node, null);
                e.Handled = true;
                return;
            }
        };

        // simple right-click menu
        treeItemMenu = new ContextMenu();
        ReactiveCommand<string, Unit> command = ReactiveCommand.Create<string>(PerformCommand);
        treeItemMenu.Items.Add(new MenuItem { Command = command, CommandParameter = "helloworld", Header = "Hello world" });

        UpdateExplore();
    }

    void HandleNodeSelected(ExplorerNode explorerNode, TreeViewItem viewItem)
    {
        if (explorerNode == null) return;

        if (explorerNode.IsDirectory)
        {
            // if TreeViewItem available, toggle expand. Otherwise nothing
            if (viewItem != null)
                viewItem.IsExpanded = !viewItem.IsExpanded;
        }
        else
        {
            CommandManager.ExecuteCommand("editor.action.open", explorerNode.Path);
        }
    }

    void PerformCommand(string commandId)
    {
        CommandEntry commandEntry = CommandManager.GetCommandEntry(commandId);
        if (commandEntry == null)
        {
            DebugWriter.WriteLine("KeybindingManager", $"command Entry is null from {commandId}");
            return;
        }

        CommandManager.ExecuteCommandGetArgs(commandId);
    }

    public void UpdateExplore()
    {
        if (string.IsNullOrEmpty(WorkspacePath))
        {
            FolderNotOpened.IsVisible = true;
            FileExplore.IsVisible = false;
        }
        else
        {
            FolderNotOpened.IsVisible = false;
            FileExplore.IsVisible = true;
        }
    }

    public void OpenFolder(string path = "")
    {
        Stopwatch sw = Stopwatch.StartNew();
        if (string.IsNullOrEmpty(path))
        {
            path = MainWindow.OpenDialog(DialogType.SelectFolder);
            if (string.IsNullOrEmpty(path)) return;
            CommandManager.ExecuteCommand("editor.action.open.folder", path);
        }

        AppPaths.SetWorkspacePath(path);
        WorkspacePath = path;
        DebugWriter.WriteLine("Explorer", $"Opened Workspace: {WorkspacePath}");
        UpdateExplore();
        UpdateTreeContents();
        sw.Stop();
        DebugWriter.WriteLine("Explorer", $"OpenFolder: Elapsed {sw.ElapsedMilliseconds} ms path = {path}");
    }

    public void UpdateTreeContents()
    {
        try
        {
            treeView.Items.Clear();

            if (string.IsNullOrEmpty(WorkspacePath) || !Directory.Exists(WorkspacePath))
            {
                // nothing to show
                FileExplore.Child = null;
                FolderNotOpened.IsVisible = true;
                FileExplore.IsVisible = false;
                return;
            }

            // TODO Build asynchronously at some point
            Stopwatch buildSw = Stopwatch.StartNew();
            ExplorerNode rootNode = BuildNode(WorkspacePath, true);
            TreeViewItem rootItem = BuildNodeContents(rootNode, 0, true);
            buildSw.Stop();

            Stopwatch addSw = Stopwatch.StartNew();
            // create a stackpanel to hold treeView (allow scroll)
            treeView.Items.Add(rootItem);
            scrollViewer.Content = treeView;
            FileExplore.Child = scrollViewer;
            addSw.Stop();

            DebugWriter.WriteLine("Explorer", $"builds: Elapsed {buildSw.ElapsedMilliseconds} ms; add: {addSw.ElapsedMilliseconds} ms");
        }
        catch (Exception ex)
        {
            DebugWriter.WriteLine("Explorer", $"UpdateTreeContents error: {ex.Message}");
        }

        UpdateSettings();
    }

    public override void UpdateSettings()
    {
        // Safe resource casts using Application.Current.Resources.GetResource
        button.Foreground = Application.Current.Resources.GetResource("button.foreground");

        button.Background = Application.Current.Resources.GetResource("button.background");

        button.AddHoverBackground("button.background", "button.hover.background");
        textBlock.Foreground = Application.Current.Resources.GetResource("sidepanel.foreground") as IBrush;
        MainPanel.Background = Application.Current.Resources.GetResource("sidepanel.background") as IBrush;
        Background = Application.Current.Resources.GetResource("sidepanel.background") as IBrush;

        if (FileExplore.Child != null && ElementSize.Width > 0 && ElementSize.Height > 0)
        {
            Height = ElementSize.Height;
            scrollViewer.Height = Height;
        }
    }

    public ExplorerNode BuildNode(string path, bool first = false)
    {
        Stopwatch stopwatch = null;
        if (first) stopwatch = Stopwatch.StartNew();

        ExplorerNode node = new ExplorerNode
        {
            Header = Path.GetFileName(path) == string.Empty ? path : Path.GetFileName(path),
            Path = path,
            IsDirectory = Directory.Exists(path)
        };

        if (node.IsDirectory)
        {
            try
            {
                IOrderedEnumerable<string> dirs = Directory.EnumerateDirectories(path).OrderBy(d => d);
                foreach (string dir in dirs)
                {
                    // guard against long recursions / permissions
                    try { node.Children.Add(BuildNode(dir)); }
                    catch { /* ignore inaccessible directories */ }
                }

                IOrderedEnumerable<string> files = Directory.EnumerateFiles(path).OrderBy(f => f);
                foreach (string file in files)
                {
                    node.Children.Add(new ExplorerNode
                    {
                        Header = Path.GetFileName(file),
                        Path = file,
                        IsDirectory = false
                    });
                }
            }
            catch (UnauthorizedAccessException) { /* ignore */ }
            catch (PathTooLongException) { /* ignore */ }
            catch (Exception ex) { DebugWriter.WriteLine("Explorer", $"BuildNode exception: {ex.Message}"); }
        }

        if (first) stopwatch?.Stop();
        if (first) DebugWriter.WriteLine("Explorer", $"BuildNode: Elapsed {stopwatch?.ElapsedMilliseconds} ms");
        return node;
    }

    public TreeViewItem BuildNodeContents(ExplorerNode node, int indent, bool first = false)
    {
        Stopwatch stopwatch = null;
        if (first) stopwatch = Stopwatch.StartNew();

        TreeViewItem root = new TreeViewItem { Header = node.Header, Tag = node, ContextMenu = treeItemMenu };

        foreach (ExplorerNode child in node.Children)
        {
            TreeViewItem childControl = BuildNodeContents(child, indent + 1);
            root.Items.Add(childControl);
        }

        if (first) stopwatch?.Stop();
        if (first) DebugWriter.WriteLine("Explorer", $"BuildNodeContents: Elapsed {stopwatch?.ElapsedMilliseconds} ms");
        return root;
    }

    public void UpdateNode(string path, string value)
    {
        // TODO: incremental update (e.g. add/remove single node)
    }
}

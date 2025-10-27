
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
using Avalonia.Interactivity;
using CodeEditor;
using System.Diagnostics;
using System.Runtime.InteropServices;
public enum DialogType
{
    OpenFile,
    SaveFile,
    SelectFolder
}

public partial class MainWindow : Window
{
    public static SettingsManager<EditorConfigs> EditorConfigsSettingsManager { get; set; }
    public static SettingsManager<GlobalStorageSettings> GlobalStorageSettingsManager { get; set; }

    public SidePanel LeftSidePanel;
    public Editor CodeEditor;
    public SidePanel RightSidePanel;

    public TopPalette TopPalette;

    public Explorer Explorer;

    public Canvas Overlay;
    public TopBar TopMenu;

    public StatusBar StatusBar;
    public static Process[] processes;
    public MainWindow()
    {
        // StartOffSync();
        CommandManager.RegisterCommand("Say Hello World", "helloworld", () => { DebugWriter.WriteLine("Window", "Hello world editor command"); });
        CommandManager.RegisterCommand("Say Hello World", "helloworld.global", () => { DebugWriter.WriteLine("Window", "Hello world global command"); });

        PixelSize screenSize = Screens.Primary.Bounds.Size;
        EditorConfigsSettingsManager = new SettingsManager<EditorConfigs>("", AppPaths.GlobalConfigFilePath);
        GlobalStorageSettingsManager = new SettingsManager<GlobalStorageSettings>(AppPaths.GlobalStorageFilePath);

        EditorConfigsSettingsManager.Current.Editor = new EditorSection();
        EditorConfigsSettingsManager.Current.Editor.FontSize = 12;
        EditorConfigsSettingsManager.Current.Editor.FontFamily = "Consolas";
        EditorConfigsSettingsManager.Current.Editor.IndentWidth = 4;
        EditorConfigsSettingsManager.Current.Editor.InsertSpaces = true;
        EditorConfigsSettingsManager.Load();

        GlobalStorageSettingsManager.Load();

        KeybindingManager.AttachToWindow(this);
        AppPaths.EnsureDirectoriesExist();
        AppPaths.EnsureFilesExist();

        string path = Path.Combine(AppPaths.ThemesDirectoryPath, "DefaultDark.json");
        if (!File.Exists(path))
        {
            File.WriteAllBytes(path, Resource.GetImage("DefaultDarkTheme"));
        }

        if (!string.IsNullOrEmpty(GlobalStorageSettingsManager.Current.DefaultTheme))
        {
            SetTheme(GlobalStorageSettingsManager.Current.DefaultTheme);
        }
        else
        {
            SetTheme("DefaultDark");
        }
        Application.Current.Styles.Add(ThemeService.ThemeStyles);

        InitializeComponent();
        ThemeService.SetRegistryOptions(CodeEditor);

        UpdateSettings();

        EditorConfigsSettingsManager.OnConfigChanged += OnEditorConfigsChanged;

        ReactiveCommand<Unit, Unit> OpenPaltteCommand = ReactiveCommand.Create(OpenPalette);
        KeyBindings.Add(new KeyBinding() { Gesture = new KeyGesture(Key.P, KeyModifiers.Control | KeyModifiers.Shift), Command = OpenPaltteCommand });

        bool firstTimePass = true;
        SizeChanged += (sender, args) =>
        {
            if (TopPalette != null)
            {
                Size topbarSize = TopMenu.Bounds.Size;
                TopPalette.WindowChangedSize(args.NewSize, screenSize, topbarSize);

                Size editorSize = CodeEditor.Bounds.Size;

                Size leftSideSize = LeftSidePanel.Bounds.Size;
                Size rightSizeSize = RightSidePanel.Bounds.Size;
                LeftSidePanel.WindowChangedSize(args.NewSize, rightSizeSize);
                RightSidePanel.WindowChangedSize(args.NewSize, leftSideSize);

                if (firstTimePass == true)
                {
                    LeftSidePanel.UpdateSettings();
                    RightSidePanel.UpdateSettings();
                    Explorer.UpdateTreeContents();
                    firstTimePass = false;
                }
            }
        };

        CommandManager.RegisterCommand("Open file", "editor.action.open.file", OpenFileDialog);
        CommandManager.RegisterCommand("Open file", "editor.action.open", OpenFile);
        CommandManager.RegisterCommand("Open folder", "editor.action.open.folder", OpenFolder);


        CommandManager.RegisterCommand("Increase Editor Font Size", "editor.action.increase.fontsize", CodeEditor.IncreaseEditorFontSize);
        CommandManager.RegisterCommand("Decrease Editor Font Size", "editor.action.decrease.fontsize", CodeEditor.DecreaseEditorFontSize);
        CommandManager.RegisterCommand("File: Save", "editor.action.file.save", SaveFile);
        CommandManager.RegisterCommand("File: Save As...", "editor.action.file.saveAs", SaveFileAs);
        CommandManager.RegisterCommand("File: New Untitled Text File", "editor.action.file.newUntitledFile", CodeEditor.NewTab);
        CommandManager.RegisterCommand("View: Close Editor", "editor.action.closeActiveEditor", CodeEditor.CloseTab);
        CommandManager.RegisterCommand("View: Toggle Primary Side Panel Visibility", "editor.action.primaryVisibility", RightSidePanel.Toggle);
        CommandManager.RegisterCommand("View: Toggle Secondary Side Panel Visibility", "editor.action.secondaryVisibility", LeftSidePanel.Toggle);
        CommandManager.RegisterCommand("Format Document", "editor.action.formatDocument", IndentDocument);
        CommandManager.RegisterCommand("View: Pin editor", "editor.action.pinEditor", PinTab);
        CommandManager.RegisterCommand("View: Unpin editor", "editor.action.unpinEditor", UnpinTab);

        CommandManager.RegisterCommand("Add control to Status bar", "view.status.add.text", StatusBar.AddText);
        CommandManager.RegisterCommand("Add control to Status bar", "view.status.add.button", StatusBar.AddButton);

        CommandManager.RegisterCommand("Open Palette", "view.open.toppalette", OpenPalette);

        if (GlobalStorageSettingsManager.Current.RecentFolders.Count > 0)
        {
            for (int i = 0; i < GlobalStorageSettingsManager.Current.RecentFolders.Count; i++)
            {
                string item = GlobalStorageSettingsManager.Current.RecentFolders[i];
                if (!Directory.Exists(item))
                {
                    continue;
                }
                CommandManager.ExecuteCommand("editor.action.open.folder", item);
            }
        }

        ExtensionManager.LoadExtensions();
    }
/*
    static async Process[] StartOffSync()
    {
        List<Process> processes = new List<Process>();

        processes.Add(await Task.Run(async () =>
        {
            return await LanguageToolManager.EnsureLanguageToolsAsync("javascript");
        }));
        return processes.ToArray();
    }
*/

    public void InitializeComponent()
    {
        Width = 800;
        Height = 600;

        MinWidth = 600;
        MinHeight = 700;
        Title = "Code Editor";

        if (EditorConfigsSettingsManager.Current.Editor == null)
        {
            // TODO add in a default config file in some way 
            EditorConfigsSettingsManager.Current.Editor = new EditorSection();
        }

        Application.Current.Resources.Add("editor.fontsize", EditorConfigsSettingsManager.Current.Editor.FontSize);
        Application.Current.Resources.Add("editor.font", EditorConfigsSettingsManager.Current.Editor.FontFamily);

        Grid rootGrid = new Grid
        {
            // Background = new SolidColorBrush(Color.Parse("#333333")) // window background
            Background = Brushes.White, // window background
        };
        rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // top menu height
        rootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star))); // rest of window

        TopMenu = new TopBar()
        {
            IsVisible = true,
            Height = 32
        };
        Grid.SetRow(TopMenu, 0);
        rootGrid.Children.Add(TopMenu);

        BuildMenu();

        DockPanel mainDock = new DockPanel()
        {
            Background = Brushes.Transparent,
        };
        Grid.SetRow(mainDock, 1);
        rootGrid.Children.Add(mainDock);

        Control mainContents = CreateLayout();
        mainDock.Children.Add(mainContents);

        Overlay = new Canvas
        {
            VerticalAlignment = VerticalAlignment.Top,
            Background = Brushes.Transparent,
            IsVisible = true // only visible when a submenu is open
        };

        TopMenu.SetSubmenuHost(Overlay);

        TopPalette = new TopPalette(Overlay);
        TopPalette.AddElement(new CommandPalette());

        // Overlay spans both rows
        Grid.SetRowSpan(Overlay, 2);
        rootGrid.Children.Add(Overlay);

        Content = rootGrid;

        Closed += OnWindowClosed;
        PointerPressed += (s, e) =>
        {
            if (Overlay.IsVisible == true && Overlay.IsPointerOver == false)
            {
                e.Handled = true;
                TopMenu.HideAllSubmenus();
                TopPalette.ClosePalette();
            }
        };
    }

    void BuildMenu()
    {
        TopBarMenu fileMenu = TopMenu.AddMenu("File");
        fileMenu.AddItem("New Text File", () => { DebugWriter.WriteLine("Main", $"New file"); });

        fileMenu.AddSeparator();

        fileMenu.AddItem("Open File", () =>
        {
            string path = OpenDialog(DialogType.OpenFile);
            if (path == null)
            {
                return;
            }
            CommandManager.ExecuteCommand("editor.action.open", path);
        });
        fileMenu.AddItem("Open Folder", () =>
        {
            string path = OpenDialog(DialogType.SelectFolder);
            if (path == null)
            {
                return;
            }
            CommandManager.ExecuteCommand("editor.action.open.folder", path);
        });

        TopBarMenu openReacentSubMenu = fileMenu.AddSubMenu("Open Reacent");
        openReacentSubMenu.AddItem("Reopen closed Editors", () => { });
        if (GlobalStorageSettingsManager.Current.RecentFolders.Count > 0)
        {
            openReacentSubMenu.AddSeparator();
            List<string> recentFolders = GlobalStorageSettingsManager.Current.RecentFolders;
            foreach (string folder in recentFolders)
            {
                openReacentSubMenu.AddItem(folder, () => { CommandManager.ExecuteCommand("editor.action.open.folder", folder); });
            }
        }
        if (GlobalStorageSettingsManager.Current.RecentWorkspaces.Count > 0)
        {
            openReacentSubMenu.AddSeparator();
            List<string> recentFolders = GlobalStorageSettingsManager.Current.RecentWorkspaces;
            foreach (string folder in recentFolders)
            {
                openReacentSubMenu.AddItem(folder, () => { CommandManager.ExecuteCommand("editor.action.open.folder", folder); });
            }
        }
        openReacentSubMenu.AddSeparator();
        openReacentSubMenu.AddItem("More...", () => { });
        openReacentSubMenu.AddSeparator();
        openReacentSubMenu.AddItem("Clear Recently Opened...", () =>
        {
            GlobalStorageSettingsManager.Current.RecentFiles.Clear();
            GlobalStorageSettingsManager.Current.RecentFolders.Clear();
            GlobalStorageSettingsManager.Current.RecentWorkspaces.Clear();
            GlobalStorageSettingsManager.SaveGlobal();
        });

        TopBarMenu EditMenu = TopMenu.AddMenu("Edit");

        TopBarMenu SelectionMenu = TopMenu.AddMenu("Selection");

        TopBarMenu ViewMenu = TopMenu.AddMenu("View");

        TopBarMenu HelpMenu = TopMenu.AddMenu("Help");

    }

    public override void EndInit()
    {
        base.EndInit();
        UpdateSettings();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateSettings();
    }

    public void OnWindowClosed(object sender, EventArgs e)
    {
        GlobalStorageSettingsManager.SaveGlobal();
        EditorConfigsSettingsManager.SaveGlobal();
        ExtensionManager.DeactivateAll();
    }

    Control CreateLayout()
    {
        Grid mainGrid = new Grid();

        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(250))); // Left panel
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));     // Left splitter
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star))); // Editor
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));     // Right splitter
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(250))); // Right panel

        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // Other
        mainGrid.RowDefinitions.Add(new RowDefinition(new GridLength(20))); // Status bar


        // Left panel
        LeftSidePanel = new SidePanel(Dock.Left);

        Explorer = new Explorer();
        LeftSidePanel.AddItem(Explorer);
        LeftSidePanel.AddItem("Search", new TextBlock { Text = "Search content" });
        Grid.SetRow(LeftSidePanel, 0);
        Grid.SetColumn(LeftSidePanel, 0);
        mainGrid.Children.Add(LeftSidePanel);

        // Attach panel to its parent column so collapse/expand works automatically
        LeftSidePanel.AttachToColumn(mainGrid.ColumnDefinitions[0]);

        // Add splitter from SidePanel to parent Grid
        Grid.SetRow(LeftSidePanel.Splitter, 0);
        Grid.SetColumn(LeftSidePanel.Splitter, 1);
        mainGrid.Children.Add(LeftSidePanel.Splitter);

        // Editor
        CodeEditor = new Editor()
        {
            Name = "Editor"
        };
        Grid.SetRow(CodeEditor, 0);
        Grid.SetColumn(CodeEditor, 2);
        mainGrid.Children.Add(CodeEditor);

        // Right panel
        RightSidePanel = new SidePanel(Dock.Right);
        RightSidePanel.AddItem("Outline", new TextBlock { Text = "Outline content" });
        RightSidePanel.AddItem("Properties", new TextBlock { Text = "Properties content" });
        Grid.SetRow(RightSidePanel, 0);
        Grid.SetColumn(RightSidePanel, 4);
        mainGrid.Children.Add(RightSidePanel);

        // Add right splitter
        Grid.SetRow(RightSidePanel.Splitter, 0);
        Grid.SetColumn(RightSidePanel.Splitter, 3);
        mainGrid.Children.Add(RightSidePanel.Splitter);

        RightSidePanel.AttachToColumn(mainGrid.ColumnDefinitions[4]);

        StatusBar = new StatusBar();
        Grid.SetRow(StatusBar, 1);
        Grid.SetColumnSpan(StatusBar, 5);
        mainGrid.Children.Add(StatusBar);

        return mainGrid;
    }

    public void SetTheme(string theme)
    {
        ThemeService.SetTheme(theme);
        GlobalStorageSettingsManager.Current.DefaultTheme = theme;
        GlobalStorageSettingsManager.SaveGlobal();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        CodeEditor.OnClosed();
    }

    void UpdateKeyDown(object sender, KeyEventArgs e)
    {
        /*
        if (isKeyDown(Key.O, e, KeyModifiers.Control))
        {
            string path = OpenDialog(DialogType.OpenFile);
            if (!string.IsNullOrEmpty(path))
            {
                CodeEditor.OpenFile(path);
            }
        }
        if (isKeyDown(Key.OemPlus, e, KeyModifiers.Control))
        {
            int fontsize = (int)Application.Current.Resources["editor.fontsize"];
            fontsize += 1;
            Application.Current.Resources["editor.fontsize"] = fontsize;
            CodeEditor.UpdateSettings();
        }
        if (isKeyDown(Key.OemMinus, e, KeyModifiers.Control))
        {
            int fontsize = (int)Application.Current.Resources["editor.fontsize"];
            fontsize -= 1;
            if (fontsize <= 0) fontsize = 1;
            Application.Current.Resources["editor.fontsize"] = fontsize;
            CodeEditor.UpdateSettings();
        }
        if (isKeyDown(Key.S, e, KeyModifiers.Control))
        {
            CodeEditor.SaveFile();
        }
        if (isKeyDown(Key.S, e, KeyModifiers.Control, KeyModifiers.Shift))
        {
            string path = OpenDialog(DialogType.SaveFile);
            CodeEditor.SaveFile(path);
        }
        if (isKeyDown(Key.W, e, KeyModifiers.Control))
        {
            CodeEditor.CloseTab();
        }
        if (isKeyDown(Key.N, e, KeyModifiers.Control))
        {
            CodeEditor.NewTab();
        }
        if (isKeyDown(Key.B, e, KeyModifiers.Control))
        {
            RightSidePanel.Toggle();
        }
        if (isKeyDown(Key.B, e, KeyModifiers.Control, KeyModifiers.Alt))
        {
            LeftSidePanel.Toggle();
        }
        if (isKeyDown(Key.F, e, KeyModifiers.Control, KeyModifiers.Alt))
        {
            CodeEditor.IndentDocument();
        }
        */

        TopPalette.OnKeyDownPalette(sender, e);
    }

    public void SetNewTheme()
    {
        string[] themes = Directory.GetFiles(AppPaths.ThemesDirectoryPath, "*.json");
        TopPalette.OpenPalette("list", SetTheme, themes);


    }

    public void OpenPalette()
    {
        TopPalette.OpenPalette("cmd");
    }

    public void OnEditorConfigsChanged()
    {
        CodeEditor.OnConfigChanged();
    }

    public void UpdateSettings()
    {
        CodeEditor.UpdateSettings();
        LeftSidePanel.UpdateSettings();
        RightSidePanel.UpdateSettings();
        StatusBar.UpdateSettings();
    }

    public static string OpenDialog(DialogType type, string defaultPath = null)
    {
        DialogResult dialog;
        string result = "";
        if (type == DialogType.OpenFile)
        {
            dialog = Dialog.FileOpen(defaultPath: defaultPath);
            if (dialog.IsOk) result = dialog.Path;
        }
        else if (type == DialogType.SaveFile)
        {
            dialog = Dialog.FileSave(defaultPath: defaultPath);
            if (dialog.IsOk) result = dialog.Path;
        }
        else if (type == DialogType.SelectFolder)
        {
            dialog = Dialog.FolderPicker(defaultPath: defaultPath);
            if (dialog.IsOk) result = dialog.Path;
        }

        return result;
    }

    public override void Show()
    {
        base.Show();
    }
}
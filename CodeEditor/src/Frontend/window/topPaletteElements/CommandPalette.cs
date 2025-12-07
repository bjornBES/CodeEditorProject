
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;

public class CommandPalette : TopPaletteElement
{
    public override string ElementName { get; protected set; }
    public List<CommandEntry> CommandEntries { get; set; }

    StackPanel mainPanel;

    ScrollViewer scrollViewer;

    StackPanel commandList;

    TextBox inputBox;
    const int MaxCommandCount = 15;

    public CommandPalette()
    {
        CommandEntries = new List<CommandEntry>();
        ElementName = "cmd";

        InitializeComponent();
    }

    public void InitializeComponent()
    {
        Background = Brushes.Black;

        mainPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
        };

        scrollViewer = new ScrollViewer();

        commandList = new StackPanel()
        {
            Orientation = Orientation.Vertical,
        };
        scrollViewer.Content = commandList;

        inputBox = new TextBox();
        mainPanel.Children.Add(inputBox);
        mainPanel.Children.Add(scrollViewer);
        Children.Add(mainPanel);
    }

    public override void OpenElement()
    {
        CommandEntries.Clear();
        commandList.Children.Clear();
        inputBox.Focus();

        scrollViewer.Height = Height;

        int count = 0;
        List<CommandEntry> entries = CommandManager.commandEntries;
        foreach (CommandEntry entry in entries)
        {
            Brush defaultColor = Application.Current.Resources.GetResource("topPanel.commandPalette.button.background", "topPanel.button.background");
            Brush hoverColor = Application.Current.Resources.GetResource("topPanel.commandPalette.button.hover.background", "topPanel.button.hover.background");
            Brush focusColor = Application.Current.Resources.GetResource("topPanel.commandPalette.button.focus.background", "topPanel.button.focus.background");

            Button button = new Button()
            {
                Name = $"CMDButton_{count}",
                Content = entry.DisplayName,
                Padding = new Thickness(0, 0),
                Background = defaultColor,
            };
            count++;
            button.AddPseudoClassesBackground(defaultColor, hoverColor, "pointerover");
            button.AddPseudoClassesBackground(defaultColor, focusColor, "focus");


            button.Click += (s, e) =>
            {
                CommandManager.ExecuteCommandGetArgs(entry.CommandId);
                ClosePalette.Invoke();
            };
            commandList.Children.Add(button);
            CommandEntries.Add(entry);
        }
    }
}
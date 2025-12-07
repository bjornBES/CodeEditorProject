
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

public class ListPalette : TopPaletteElement
{
    public override string ElementName { get; protected set; }
    public List<CommandEntry> CommandEntries{ get; set; }

    StackPanel mainPanel;

    StackPanel commandList;

    TextBox inputBox;
    const int MaxCommandCount = 15;

    public ListPalette()
    {
        CommandEntries = new List<CommandEntry>();
        ElementName = "list";

        InitializeComponent();
    }

    public void InitializeComponent()
    {
        Background = Brushes.Black;

        mainPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
        };

        commandList = new StackPanel()
        {
            Orientation = Orientation.Vertical,
        };

        inputBox = new TextBox();
        mainPanel.Children.Add(inputBox);
        mainPanel.Children.Add(commandList);
        Children.Add(mainPanel);
    }

    public override void OpenElement<T>(Action<T> returnAction, T[] list)
    {
        CommandEntries.Clear();
        commandList.Children.Clear();
        inputBox.Focus();

        foreach (T entry in list)
        {
            Button button = new Button()
            {
                Content = entry,
                Padding = new Thickness(0, 0)
            };
            button.Click += (s, e) =>
            {
                returnAction.Invoke(entry);
                ClosePalette.Invoke();
            };
            commandList.Children.Add(button);
            if (CommandEntries.Count == MaxCommandCount)
            {
                break;
            }
        }
    }
}
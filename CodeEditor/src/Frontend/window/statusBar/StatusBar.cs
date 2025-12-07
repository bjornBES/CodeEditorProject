
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

public class StatusBar : ControlElement<StatusBar>
{
    Border mainBorder;
    Grid mainGrid;
    StackPanel leftPanel;
    StackPanel rightPanel;

    List<TextBlock> textControls = new List<TextBlock>();
    List<Button> buttonControls = new List<Button>();
    public StatusBar()
    {
        mainBorder = new Border()
        {
            Background = Application.Current.Resources.GetResource("statusBar.border.background"),
            BorderThickness = new Thickness(0, 1, 0, 0)
        };

        mainGrid = new Grid()
        {
            ZIndex = 1,
            Background = Application.Current.Resources.GetResource("statusBar.background"),
            ColumnDefinitions = {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        mainBorder.Child = mainGrid;

        leftPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(4, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        mainGrid.Children.Add(leftPanel);
        Grid.SetColumn(leftPanel, 0);

        rightPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        mainGrid.Children.Add(rightPanel);
        Grid.SetColumn(rightPanel, 2);

        Children.Add(mainBorder);
    }

    public void AddText(TextBlock control, Dock dock)
    {
        textControls.Add(control);
        if (dock == Dock.Left)
        {
            leftPanel.Children.Add(control);
        }
        else if (dock == Dock.Right)
        {
            rightPanel.Children.Add(control);
        }
    }

    public void AddButton(Button control, Dock dock)
    {
        buttonControls.Add(control);
        if (dock == Dock.Left)
        {
            leftPanel.Children.Add(control);
        }
        else if (dock == Dock.Right)
        {
            rightPanel.Children.Add(control);
        }
    }

    public void UpdateSettings()
    {
        mainBorder.Background = Application.Current.Resources.GetResource("statusBar.border.background");
        mainGrid.Background = Application.Current.Resources.GetResource("statusBar.background");

        foreach (TextBlock control in textControls)
        {
            control.Foreground = Application.Current.Resources.GetResource("statusBar.foreground");
        }
        foreach (Button control in buttonControls)
        {
            control.Foreground = Application.Current.Resources.GetResource("statusBar.foreground");
        }
    }
}
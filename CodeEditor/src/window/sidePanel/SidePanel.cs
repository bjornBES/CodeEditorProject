using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SidePanel : ControlElement<SidePanel>
{
    List<SidePanelElement> panelElements = new List<SidePanelElement>();

    public Grid mainGrid;
    private TabControl tabControl;
    private Button toggleButton;

    public GridSplitter Splitter;
    public Dock Dock;
    private GridLength originalColumnWidth = new GridLength(250, GridUnitType.Pixel);
    private bool isCollapsed = false;

    public double innerHeight;

    // Events
    public event Action Collapsed;
    public event Action Expanded;

    public SidePanel(Dock dock, double width = 250)
    {
        Dock = dock;
        originalColumnWidth = new GridLength(width, GridUnitType.Pixel);
        MinWidth = 170;
        Initialize();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Background resource
        Background = Application.Current.Resources.GetResource("sidepanel.background");

        mainGrid = new Grid();

        // Two rows: top for toggle button, rest for content
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        mainGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));

        // TabControl for tabs
        tabControl = new TabControl();
        Grid.SetRow(tabControl, 1);
        tabControl.HorizontalAlignment = HorizontalAlignment.Stretch;
        tabControl.VerticalAlignment = VerticalAlignment.Stretch;
        mainGrid.Children.Add(tabControl);

        tabControl.SizeChanged += (s, e) =>
        {
            // keep TabControl filling the row; don't set fixed width from event
            e.Handled = true;
        };

        // Toggle button
        toggleButton = new Button
        {
            Width = 20,
            Height = 20,
            Content = isCollapsed ? "▶" : "◀",
            HorizontalAlignment = Dock == Dock.Left ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2)
        };
        toggleButton.Click += (_, __) => Toggle();
        Grid.SetRow(toggleButton, 0);
        mainGrid.Children.Add(toggleButton);

        // GridSplitter: note - in many layouts the splitter belongs in the parent grid; we keep a splitter here in case
        Splitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new SolidColorBrush(Colors.Gray)
        };

        // SizeChanged handler for the control itself
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == nameof(Width) || e.Property.Name == nameof(Height))
            {
                // update element sizes for children
                foreach (SidePanelElement item in panelElements)
                {
                    // give elements full available size (you can subtract tab header height if you want)
                    item.ElementSize = new Size(Width, Height);
                }
            }
        };

        this.Children.Add(mainGrid); // if ControlElement<> is a ContentControl; if not, keep Children.Add(mainGrid)
        // If it is not a ContentControl, revert to: Children.Add(mainGrid);
    }

    private ColumnDefinition parentColumn;

    public void AttachToColumn(ColumnDefinition column)
    {
        parentColumn = column;
        // if parentColumn has a width already, store it
        originalColumnWidth = parentColumn.Width.IsStar || parentColumn.Width.IsAuto
            ? parentColumn.Width
            : new GridLength(parentColumn.Width.Value, GridUnitType.Pixel);
    }

    public void AddItem(string header, Control control)
    {
        TabItem tabItem = new TabItem() { Header = header, Content = control };
        tabControl.Items.Add(tabItem);
    }

    public void AddItem(SidePanelElement element)
    {
        TabItem tabItem = new TabItem()
        {
            Header = element.Header,
            Content = element,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Name = element.Header
        };

        tabItem.GotFocus += (s, args) =>
        {
            // handle selection visuals via styles or resources
        };

        // optional icon header
        try
        {
            string path = Path.Combine(AppPaths.DownloadedAssetsDirectoryPath, element.IconKey);
            if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists(path))
                {
                    File.WriteAllBytes(path, CodeEditor.Resource.GetImage(element.IconKey));
                }
                tabItem.Header = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(path), Width = 20, Height = 20 };
            }
        }
        catch
        {
            // swallow resource errors and fall back to text header
        }

        element.GotFocus += (sender, e) => { KeybindingManager.ActiveContext = element.Header; };

        tabControl.Items.Add(tabItem);
        panelElements.Add(element);
    }

    public void UpdateSettings()
    {
        Background = Application.Current.Resources.GetResource("sidepanel.background");

        foreach (SidePanelElement item in panelElements)
            item.UpdateSettings();
    }

    // Collapse the panel
    public void Collapse(ColumnDefinition column = null)
    {
        ColumnDefinition col = column ?? parentColumn;
        if (col == null) return;

        // Save current width
        originalColumnWidth = col.Width;
        col.Width = new GridLength(0);
        Splitter.IsEnabled = false;
        isCollapsed = true;
        toggleButton.Content = Dock == Dock.Left ? "▶" : "◀";
        Collapsed?.Invoke();
    }

    // Expand the panel
    public void Expand(ColumnDefinition column = null)
    {
        ColumnDefinition col = column ?? parentColumn;
        if (col == null) return;

        col.Width = originalColumnWidth;
        Splitter.IsEnabled = true;
        isCollapsed = false;
        toggleButton.Content = Dock == Dock.Left ? "◀" : "▶";
        Expanded?.Invoke();
    }

    public void Toggle()
    {
        if (isCollapsed)
            Expand(parentColumn);
        else
            Collapse(parentColumn);
    }

    public void WindowChangedSize(Size windowSize, Size otherSideSize)
    {
        double availableWidth = Math.Max(0, windowSize.Width - otherSideSize.Width);

        double maxAllowedWidth = availableWidth * 0.45d;
        if (maxAllowedWidth < 170)
            maxAllowedWidth = 170;

        MaxWidth = maxAllowedWidth;
        MinWidth = 170;

        if (parentColumn != null)
        {
            parentColumn.MaxWidth = MaxWidth;
            parentColumn.MinWidth = MinWidth;
        }
    }
}

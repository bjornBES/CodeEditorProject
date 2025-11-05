using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using lib.debug;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;

public class SidePanel : ControlElement<SidePanel>
{
    List<SidePanelElement> panelElements = new List<SidePanelElement>();

    public Grid mainGrid;
    private TabControl tabControl;

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
        Initialize();
        Dock = dock;
        originalColumnWidth = new GridLength(width, GridUnitType.Pixel);
        MinWidth = 170;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Background resource
        Background = Application.Current.Resources.GetResource("sidepanel.background");

        mainGrid = new Grid();

        // Two rows: top for toggle button, rest for content
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // button
        mainGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star))); // tabs

        // TabControl for tabs
        tabControl = new TabControl();
        Grid.SetColumn(tabControl, 1);
        mainGrid.Children.Add(tabControl);

        // GridSplitter: note - in many layouts the splitter belongs in the parent grid; we keep a splitter here in case
        Splitter = new GridSplitter
        {
            Width = 5,
            Background = new SolidColorBrush(Colors.Gray),
        };

        // SizeChanged handler for the control itself
        SizeChanged += (s, e) =>
        {
            // DebugWriter.WriteLine("Side panel", $"Size {e.NewSize.Width}, {e.NewSize.Height}");
            // update element sizes for children
            foreach (SidePanelElement item in panelElements)
            {
                e.Handled = true;
                // give elements full available size (you can subtract tab header height if you want)
                item.ElementSize = new Size(e.NewSize.Width, e.NewSize.Height);
                item.Width = e.NewSize.Width;
                item.Height = e.NewSize.Height;
                item.UpdateSettings();
            }
        };

        Children.Add(mainGrid); // if ControlElement<> is a ContentControl; if not, keep Children.Add(mainGrid)
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
        TabItem tabItem = new TabItem()
        {
            Header = header,
            Content = control
        };
        tabControl.Items.Add(tabItem);
    }


    public void AddItem(SidePanelElement element)
    {
        TabItem tabItem = new TabItem()
        {
            Header = element.Header,
            Content = element,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Name = element.Header,
            BorderBrush = Application.Current.Resources.GetResource("sidepanel.icon.border.background")
        };

        tabItem.GotFocus += (s, args) =>
        {
            tabItem.BorderBrush = Application.Current.Resources.GetResource("sidepanel.icon.border.selected.background");
        };

        tabItem.LostFocus += (sender, args) =>
        {
            tabItem.BorderBrush = Application.Current.Resources.GetResource("sidepanel.icon.border.background");
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
        DebugWriter.WriteLine("Side panel", $"WindowChangedSize({windowSize}, {otherSideSize})");
        double availableWidth = Math.Max(0, windowSize.Width - otherSideSize.Width);
        DebugWriter.WriteLine("Side panel", $"availableWidth = {availableWidth}");

        double maxAllowedWidth = availableWidth * 0.45d;
        DebugWriter.WriteLine("Side panel", $"maxAllowedWidth = {maxAllowedWidth} or 170");
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

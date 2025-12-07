using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using System;
using System.Collections.Generic;

public class TopBarMenu
{
    TextBlock TextBlock;
    TopBar TopBar;
    public TopBarMenu(TopBar topBar, TextBlock textBlock)
    {
        TextBlock = textBlock;
        TopBar = topBar;
    }

    public void AddItem(string title, Action action)
    {
        TopBar.AddItem(TextBlock, title, action);
    }
    public void AddSeparator()
    {
        TopBar.AddSeparator(TextBlock);
    }

    public TopBarMenu AddSubMenu(string title)
    {
        return TopBar.AddSubMenu(TextBlock, title);
    }
}

public class TopBar : ControlElement<TopBar>
{
    private Grid mainGrid;
    private readonly StackPanel leftPanel;
    private readonly StackPanel rightPanel;

    private readonly Dictionary<Control, MenuNode> menuNodes = new();
    private readonly Dictionary<Button, Action> actionMap = new Dictionary<Button, Action>();
    private Panel submenuHost;

    public TopBar()
    {
        Initialize();
        mainGrid = new Grid()
        {
            ZIndex = 1,
            Background = new SolidColorBrush(Color.Parse("#2d2d2d")),
            MinHeight = 32,
        };

        // Define grid layout: Left | Spacer | Right
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(4, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        mainGrid.Children.Add(leftPanel);
        Grid.SetColumn(leftPanel, 0);

        rightPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        mainGrid.Children.Add(rightPanel);
        Grid.SetColumn(rightPanel, 2);
        Children.Add(mainGrid);
    }

    public void SetSubmenuHost(Panel host)
    {
        submenuHost = host;
        submenuHost.IsVisible = false;
        submenuHost.Background = Brushes.Transparent;
        // submenuHost.PointerExited += (_, __) => HideAllSubmenus();
        // submenuHost.PointerMoved += (_, __) =>
        // {
        //     if (!submenuHost.IsPointerOver)
        //     {
        //         HideAllSubmenus();
        //     }
        // };
        submenuHost.PointerPressed += (_, e) =>
        {
            if (!submenuHost.IsPointerOver)
            {
                e.Handled = true;
                HideAllSubmenus();
            }
        };
    }

    // -------- Public API --------

    public TopBarMenu AddMenu(string title, bool alignRight = false)
    {
        TextBlock trigger = MakeTrigger(title);
        Border submenu = MakeSubmenu();
        MenuNode node = new MenuNode(trigger, submenu);

        menuNodes[trigger] = node;

        trigger.PointerPressed += (_, e) =>
        {
            if (trigger.IsPointerOver)
            {
                e.Handled = true;
                ShowSubmenu(node);
            }
        };

        GetPanel(alignRight).Children.Add(trigger);
        return new TopBarMenu(this, trigger);
    }


    public void AddMenu(string title, string[] items, Action[] actions, bool alignRight = false)
    {
        if (items.Length != actions.Length)
            throw new ArgumentException("Items and actions must match length.");

        TextBlock trigger = MakeTrigger(title);
        Border submenu = MakeSubmenu();
        MenuNode node = new MenuNode(trigger, submenu);
        StackPanel container = (StackPanel)submenu.Child;

        for (int i = 0; i < items.Length; i++)
        {
            Button button = MakeButton(items[i], actions[i]);
            container.Children.Add(button);
            actionMap[button] = actions[i];
        }

        menuNodes[trigger] = node;
        // trigger.PointerEntered += (_, __) => ShowSubmenu(trigger, submenu);
        trigger.PointerPressed += (_, e) =>
        {
            if (trigger.IsPointerOver)
            {
                e.Handled = true;
                ShowSubmenu(node, submenu);
            }
        };

        GetPanel(alignRight).Children.Add(trigger);
    }

    public void AddItem(TextBlock trigger, string item, Action action)
    {
        if (!menuNodes.ContainsKey(trigger))
            return;

        MenuNode node = menuNodes[trigger];
        StackPanel container = (StackPanel)node.Submenu.Child;

        Button button = MakeButton(item, action);
        container.Children.Add(button);
        actionMap[button] = action;
    }

    public TopBarMenu AddSubMenu(TextBlock parentTrigger, string title)
    {
        if (!menuNodes.TryGetValue(parentTrigger, out MenuNode parentNode))
            return null;

        TextBlock subtrigger = MakeTrigger(title, false);
        Border subsubmenu = MakeSubmenu();
        MenuNode childNode = new MenuNode(subtrigger, subsubmenu);

        parentNode.Children.Add(childNode);
        menuNodes[subtrigger] = childNode;

        subtrigger.PointerPressed += (_, e) =>
        {
            if (subtrigger.IsPointerOver)
            {
                e.Handled = true;
                ShowSubmenu(childNode, parentNode.Submenu);
            }
        };
        subtrigger.PointerEntered += (_, e) =>
        {
            if (subtrigger.IsPointerOver)
            {
                e.Handled = true;
                ShowSubmenu(childNode, parentNode.Submenu);
            }
        };

        ((StackPanel)parentNode.Submenu.Child).Children.Add(subtrigger);
        return new TopBarMenu(this, subtrigger);
    }


    public void AddSeparator(TextBlock trigger)
    {
        if (!menuNodes.ContainsKey(trigger))
            return;

        MenuNode node = menuNodes[trigger];
        StackPanel container = (StackPanel)node.Submenu.Child;

        Border separator = GetSeparator();

        container.Children.Add(separator);
    }

    public void AddButton(string title, Action action, bool alignRight = false)
    {
        Button button = new Button
        {
            Content = new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            },
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(10, 0),
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        button.Click += (_, __) => action?.Invoke();
        GetPanel(alignRight).Children.Add(button);
    }

    // -------- Helpers --------

    private StackPanel GetPanel(bool alignRight) => alignRight ? rightPanel : leftPanel;

    private TextBlock MakeTrigger(string title, bool root = true) => new TextBlock
    {
        Text = title,
        Padding = root == true ? new Thickness(10, 0) : new Thickness(12, 6),
        Margin = root == true ? new Thickness(10, 0, 0, 0) : new Thickness(0),
        VerticalAlignment = VerticalAlignment.Center,
        Foreground = Brushes.White,
        FontSize = 14,
        Cursor = new Cursor(StandardCursorType.Hand)
    };

    private Border MakeSubmenu() => new Border
    {
        Background = new SolidColorBrush(Color.Parse("#2d2d2d")),
        BorderBrush = Brushes.Gray,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(3),
        IsVisible = false,
        IsHitTestVisible = true,
        Child = new StackPanel { Orientation = Orientation.Vertical, Spacing = 2 }
    };

    private Button MakeButton(string text, Action action)
    {
        Button button = new Button
        {
            Content = text,
            Padding = new Thickness(12, 6),
            Background = new SolidColorBrush(Color.Parse("#2d2d2d")),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        button.Click += (_, __) =>
        {
            action?.Invoke();
            HideAllSubmenus();
        };
        return button;
    }

    private void ShowSubmenu(MenuNode node, Border parentSubmenu = null)
    {
        if (submenuHost == null)
            return;
        if (submenuHost.Children.Contains(node.Submenu))
        {
            return;
        }

        submenuHost.Children.Add(node.Submenu);

        Point? pos = parentSubmenu == null
            ? node.Trigger.TranslatePoint(new Point(0, node.Trigger.Bounds.Height), submenuHost)
            : node.Trigger.TranslatePoint(new Point(node.Trigger.Bounds.Width, 0), submenuHost);

        if (pos.HasValue)
        {
            Canvas.SetLeft(node.Submenu, pos.Value.X);
            Canvas.SetTop(node.Submenu, pos.Value.Y);
        }

        node.Submenu.IsVisible = true;
        submenuHost.IsVisible = true;
    }

    internal void HideAllSubmenus()
    {
        foreach (MenuNode node in menuNodes.Values)
            node.Submenu.IsVisible = false;

        submenuHost?.Children.Clear();
        if (submenuHost != null)
            submenuHost.IsVisible = false;
    }

    public static Border GetSeparator()
    {
        Border separator = new Border
        {
            Height = 1,
            Background = Brushes.Gray,
            Margin = new Thickness(4, 2)
        };
        return separator;
    }
}

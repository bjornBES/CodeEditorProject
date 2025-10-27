using Avalonia.Controls;

public class MenuNode
{
    public Control Trigger { get; }
    public Border Submenu { get; }
    public List<MenuNode> Children { get; } = new();

    public MenuNode(Control trigger, Border submenu)
    {
        Trigger = trigger;
        Submenu = submenu;
    }
}


using AvaloniaEdit;

public static class RoutedCommandBindingExtension
{
    // Helper for repeating strings
    public static void RemoveItem(this IList<RoutedCommandBinding> bindings, RoutedCommand command)
    {
        RoutedCommandBinding routed = null;
        foreach (RoutedCommandBinding binding in bindings)
        {
            if (binding.Command == command)
            {
                routed = binding;
                break;
            }
        }
        if (routed == null)
        {
            return;
        }
        bindings.Remove(routed);
    }
}
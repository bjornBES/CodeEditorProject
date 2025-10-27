
using Avalonia.Media;

public static class ColorExtension
{
    public static Brush GetColoredBrush(this string s)
    {
        return new SolidColorBrush(Color.Parse(s));
    }

    public static SolidColorBrush ToBrush(this string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        if (!hex.StartsWith("#")) hex = "#" + hex;
        return SolidColorBrush.Parse(hex);
    }
    public static SolidColorBrush ToBrush(this Color color)
    {
        return new SolidColorBrush(color);
    }
}
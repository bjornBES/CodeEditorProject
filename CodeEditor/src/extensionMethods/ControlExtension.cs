
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

public static class ControlExtension
{
    public static void AddHoverBackground(this TemplatedControl control, IBrush normal, IBrush hover)
    {
        StyleHelper.CreateHoverBackgroundStyle<Button>(normal, hover, control.Name);
    }
    public static void AddHoverBackground(this TemplatedControl control, string normalKey, string hoverKey)
    {
        IBrush normal = Application.Current.Resources.GetResource(normalKey);
        IBrush hover = Application.Current.Resources.GetResource(hoverKey);
        AddHoverBackground(control, normal, hover);
    }
    public static void AddHoverBackground(this TextBlock control, IBrush normal, IBrush hover)
    {
        StyleHelper.CreateHoverBackgroundStyle<TextBlock>(normal, hover, control.Name);
    }
    public static void AddHoverBackground(this TextBlock control, string normalKey, string hoverKey)
    {
        IBrush normal = Application.Current.Resources.GetResource(normalKey);
        IBrush hover = Application.Current.Resources.GetResource(hoverKey);
        AddHoverBackground(control, normal, hover);
    }
    public static void AddPseudoClassesBackground<T>(this T control, IBrush normal, IBrush other, string pseudoclass) where T : TemplatedControl
    {
        StyleHelper.CreatePseudoClassesBackgroundStyle<T>(normal, other, control.Name, pseudoclass);
    }
    public static void AddPseudoClassesBackground<T>(this T control, string normalKey, string otherKey, string pseudoclass) where T : TemplatedControl
    {
        IBrush normal = Application.Current.Resources.GetResource(normalKey);
        IBrush other = Application.Current.Resources.GetResource(otherKey);
        AddPseudoClassesBackground(control, normal, other, pseudoclass);
    }

    public static T Also<T>(this T control, Action<T> action)
    {
        action(control);
        return control;
    }
}
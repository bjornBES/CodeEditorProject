using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;
using Avalonia.Controls.Primitives;

public static class StyleHelper
{
    public static void CreateHoverBackgroundStyle<T>(IBrush normal, IBrush hover, string name) where T : Control
    {
        Application.Current.Styles.Add(new Styles
        {
            // Normal background
            new Style(x => x.OfType<T>().Name(name))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, normal),
                },
                Children = {
                    new Style(x => x.OfType<T>().Name(name).Nesting().Class(":pointerover"))
                    {
                        Setters =
                        {
                            new Setter(TemplatedControl.BackgroundProperty, hover),
                        }
                    }
                }
            },
        });
    }

    public static void CreateSelectedBackgroundStyle<T>(IBrush normal, IBrush hover, string name) where T : TabItem
    {
        Application.Current.Styles.Add(new Styles
        {
            // Normal background
            new Style(x => x.OfType<T>().Name(name))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, normal),
                },
                Children = {
                    new Style(x => x.OfType<T>().Name(name).Nesting().Class(":selected"))
                    {
                        Setters =
                        {
                            new Setter(TemplatedControl.BackgroundProperty, hover),
                        }
                    }
                }
            },
        });
    }

    public static void CreatePseudoClassesBackgroundStyle<T>(IBrush normal, IBrush other, string name, string pseudoclass) where T : TemplatedControl
    {
        Application.Current.Styles.Add(new Styles
        {
            // Normal background
            new Style(x => x.OfType<T>().Name(name))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, normal),
                },
                Children = {
                    new Style(x => x.Nesting().Class($":{pseudoclass}"))
                    {
                        Setters =
                        {
                            new Setter(TemplatedControl.BackgroundProperty, other),
                        }
                    }
                }
            },
        });
    }
}

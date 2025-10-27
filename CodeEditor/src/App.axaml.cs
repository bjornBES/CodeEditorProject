
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using Serilog;
using Serilog.Core;

public class App : Application
{

    public override void Initialize()
    {
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

/*
            Window window = new Window()
            {
                Width = 800,
                Height = 600,
            };
            AreaButton areaButton = new AreaButton()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            */
            Styles.Add(new Styles
            {
                new Style(x => x.OfType(typeof(AreaButton)))
                {
                    Setters = {
                        new Setter(AreaButton.ContentProperty, "Testing Area"),
                        new Setter(AreaButton.MinWidthProperty, 200.0d),
                        new Setter(AreaButton.BackgroundProperty, Brushes.Blue)
                    },
                    Children =
                    {
                        new Style(x => x.OfType<AreaButton>().Nesting().Class(":pointerover"))
                        {
                            Setters = {
                                new Setter(AreaButton.ContentProperty, "Pointer over"),
                                new Setter(AreaButton.BackgroundProperty, Brushes.LightBlue)
                            }
                        },
                        new Style(x => x.OfType<AreaButton>().Nesting().Class(":pointerover"))
                        {
                            Setters = {
                                new Setter(AreaButton.ContentProperty, "Pointer over"),
                                new Setter(AreaButton.BackgroundProperty, Brushes.LightBlue)
                            }
                        },

                        new Style(x => x.OfType<AreaButton>().Nesting().Class(":left"))
                        {
                            Setters = {
                                new Setter(AreaButton.ContentProperty, "Left"),
                            }
                        },
                        new Style(x => x.OfType<AreaButton>().Nesting().Class(":right"))
                        {
                            Setters = {
                                new Setter(AreaButton.ContentProperty, "Right"),
                            }
                        },
                        new Style(x => x.OfType<AreaButton>().Nesting().Class(":middle"))
                        {
                            Setters = {
                                new Setter(AreaButton.ContentProperty, "Middle"),
                            }
                        },
                    }

                }
            });
            /*


            window.Content = areaButton;

            desktop.MainWindow = window;
*/
            desktop.MainWindow.AttachDevTools();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
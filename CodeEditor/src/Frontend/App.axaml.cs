
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
                new Style(x => x.OfType<TextBlock>())
                {
                    Setters =
                    {
                        new Setter(TextBlock.FontSizeProperty, GetValue(TextBlock.FontSizeProperty) * (double)Current.Resources["allFontScale"])
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
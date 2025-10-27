
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Input;
using lib.debug;
using System.Runtime.CompilerServices;

public class TopPalette : ControlElement<TopPalette>
{
    List<TopPaletteElement> PaletteElements = new List<TopPaletteElement>();
    Canvas Overlay;
    public TopPalette(Canvas overlay)
    {
        Overlay = overlay;
        InitializeComponent();
    }

    public void InitializeComponent()
    {
        Initialize();
        MinWidth = 400;
        MaxWidth = 800;
        IsVisible = false;
        Background = Brushes.Red;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Top;
    }

    public void WindowChangedSize(Size windowSize, PixelSize screenSize, Size topbarMenuSize)
    {
        // calculate desired width/height
        double desiredWidth = windowSize.Width * 0.60d;
        double desiredHeight = windowSize.Height * 0.50d;

        // apply min/max before assigning
        Width = Math.Clamp(desiredWidth, MinWidth, MaxWidth);
        Height = desiredHeight;

        DebugWriter.WriteLine("Top palette", $"Size {Width}, {Height}");

        // center horizontally
        double left = (windowSize.Width - Width) / 2;
        Canvas.SetLeft(this, left);

        // place near top (e.g. 15% down from window top)
        double top = topbarMenuSize.Height; // coming from the top bar menu
        Canvas.SetTop(this, top);

        foreach (TopPaletteElement element in PaletteElements)
        {
            element.Width = Width;
            element.Height = Height;
        }
    }

    public void OnKeyDownPalette(object sender, KeyEventArgs e)
    {
        if (IsVisible == true)
        {
            if (e.Key == Key.Escape)
            {
                ClosePalette();
            }
        }
    }

    public void OpenPalette(string paletteName)
    {
        Overlay.IsVisible = true;
        IsVisible = true;
        Overlay.Children.Add(this);

        TopPaletteElement element = GetElement(paletteName);
        element.ClosePalette += ClosePalette;
        Children.Add(element);
        element.OpenElement();
    }

    public void OpenPalette<T>(string paletteName, Action<T> action, T[] args) where T : class
    {
        Overlay.IsVisible = true;
        IsVisible = true;
        Overlay.Children.Add(this);

        TopPaletteElement element = GetElement(paletteName);
        element.ClosePalette += ClosePalette;
        Children.Add(element);
        
        element.OpenElement(action, args);
    }

    public void ClosePalette()
    {
        Overlay.Children.Remove(this);
        Overlay.IsVisible = false;
        Children.Clear();
        IsVisible = false;
    }

    TopPaletteElement GetElement(string name)
    {
        return PaletteElements.FirstOrDefault(element => { return element.ElementName == name; });
    }

    public void AddElement(TopPaletteElement element)
    {
        PaletteElements.Add(element);
    }
}
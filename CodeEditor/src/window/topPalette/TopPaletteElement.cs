
using Avalonia.Controls;

public abstract class TopPaletteElement : Panel
{
    public abstract string ElementName { get; protected set; }
    public Action ClosePalette;
    public virtual void OpenElement()
    {

    }
    public virtual void OpenElement<T>(Action<T> returnAction, T[] list) where T : class
    {
    }
}
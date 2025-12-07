
using Avalonia.Controls;
using TextMateSharp.Grammars;

public class EditorControlView : Panel
{
    public virtual EditorInput Input{ get; set; }
    public EditorControlView(EditorInput input)
    {
        Input = input;
    }

    public virtual void InitializeSyntaxHighlighting(RegistryOptions options) {}
    public virtual void UpdateSettings() {}
    public virtual void OnConfigChanged() {}
    public virtual void ApplyTheme(RegistryOptions registryOptions) {}
    public virtual void UpdateText() {}
}


using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.TextMate;
using Microsoft.Extensions.Logging.Abstractions;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TreeSitter;

public class FileEditorView : EditorControlView
{
    public override EditorInput Input { get; set; }
    internal FileEditorInput editorInput {get => (FileEditorInput)Input; set => Input = value;}
    public readonly TextEditor Editor;
    public readonly Stream fileStream;

    private TextMate.Installation textMateInstallation;
    private TextMateSharp.Grammars.Language Language;
    private RegistryOptions registryOptions;
    private LspClient? lspClient;

    private CompletionWindow completionWindow;

    public FileEditorView(FileEditorInput input) : base(input)
    {
        Input = input;
        Focusable = true;
        Editor = new TextEditor
        {
            ShowLineNumbers = true,
            FontSize = 14,
            FontFamily = "Consolas",
            Text = input.TextContent,
            Focusable = true,
        };

        fileStream = File.Open(input.FilePath, FileMode.OpenOrCreate);

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
        Editor.TextChanged += async (_, _) => await OnEditorTextChangedAsync();
        Editor.TextArea.TextEntered += OnTextEntered;
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

        Children.Add(Editor);


    }

    public override void InitializeSyntaxHighlighting(RegistryOptions options)
    {
        registryOptions = options;
        LoadSyntaxHighlighting(options, editorInput.FilePath);
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        InitializeLspAsync(Language.Id).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    public override void UpdateText()
    {
        Editor.Text = editorInput.TextContent;
    }

    private async Task InitializeLspAsync(string languageId)
    {
    if (string.IsNullOrEmpty(languageId)) return;

    var editorParent = GetParentEditor();
    if (editorParent == null) return;

    lspClient = await editorParent.GetOrStartServerAsync(languageId);
    if (lspClient != null)
    {
        await lspClient.DidOpenAsync(editorInput.FilePath, editorInput.TextContent);
    }
    }

private Editor? GetParentEditor()
    {
        Control? parent = this;
        while (parent != null && parent != Editor)
            parent = parent.Parent as Control;
        return parent as Editor;
    }

    private async Task OnEditorTextChangedAsync()
    {
        editorInput.UpdateContent(Editor.Text);

        if (lspClient != null)
        {
            Caret caret = Editor.TextArea.Caret;
            int line = caret.Line - 1;
            int col = caret.Column - 1;
            await lspClient.DidChangeAsync(editorInput.FilePath, Editor.Text);
        }
    }

    private async void OnTextEntered(object sender, TextInputEventArgs e)
    {
        Caret caret = Editor.TextArea.Caret;
        if (e.Text == "\n")
        {
            IndentationManager.IndentAfterEnter("csharp", Editor.Document, caret.Line, 4, false);
        }
        
        if (lspClient == null) return;

        int line = caret.Line - 1;
        int col = caret.Column - 1;

        // Trigger completion on dot
        if (e.Text == ".")
        {
            await lspClient.CompletionAsync(editorInput.FilePath, line, col);
        }
    }

    public void OnClosed()
    {
        fileStream.Close();
        lspClient?.DidCloseAsync(editorInput.FilePath).GetAwaiter().GetResult();

    }

    public void UpdateFileInfo(Button textBlock)
    {
        Caret caret = Editor.TextArea.Caret;
        int line = caret.Line;
        int column = caret.Column;
        textBlock.Content = $"Ln {line}, Col {column}";
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
    }
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
    }

    public void LoadSyntaxHighlighting(RegistryOptions registryOptions, string path)
    {
        this.registryOptions = registryOptions;

        textMateInstallation = Editor.InstallTextMate(registryOptions);
        Registry registry = new Registry(registryOptions);

        TextMateSharp.Grammars.Language language = registryOptions.GetLanguageByExtension(Path.GetExtension(path));
        if (language != null)
        {
            string scopeName = registryOptions.GetScopeByLanguageId(language.Id);
            textMateInstallation.SetGrammar(scopeName);

            IGrammar grammar = registry.LoadGrammar(scopeName);
            BlockIndentationProvider blockProvider = new BlockIndentationProvider(language.Id, grammar);
            IndentationManager.RegisterProvider(blockProvider);

            Language = language;
        }
    }

    public override void ApplyTheme(RegistryOptions registryOptions)
    {
        if (Language == null) return;

        textMateInstallation = Editor.InstallTextMate(registryOptions);
        string scope = registryOptions.GetScopeByLanguageId(Language.Id);
        textMateInstallation.SetGrammar(scope);

        this.registryOptions = registryOptions;
    }

    public override void OnConfigChanged()
    {
        object fontFamily = Application.Current.Resources["editor.font"];
        Editor.FontFamily = fontFamily == null ? "Consolas" : fontFamily.ToString();

        object fontSize = Application.Current.Resources["editor.fontsize"];
        Editor.FontSize = fontSize == null ? 14 : Convert.ToDouble(fontSize);
    }

    public override void UpdateSettings()
    {
        Brush background = Application.Current.Resources.GetResource("editor.background");
        Editor.Background = background == null ? "#1f1f1f".GetColoredBrush() : background.ToString().GetColoredBrush();

        Editor.Foreground = Application.Current.Resources.GetResource("editor.foreground");

        Editor.WordWrap = MainWindow.EditorConfigsSettingsManager.Current.Editor.WordWrap;
    }

    public void Save()
    {
        using var writer = new StreamWriter(fileStream, Editor.Encoding, leaveOpen: true);
        writer.Write(editorInput.TextContent);
        writer.Flush();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        OnClosed();
    }
}

public class SimpleCompletionData : ICompletionData
{
    public SimpleCompletionData(string text)
    {
        Text = text;
    }

    IImage ICompletionData.Image { get; }
    public string Text { get; }
    public object Content => Text;
    public object Description => $"Completion: {Text}";
    public double Priority => 0;


    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }
}

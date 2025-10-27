

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.TextMate;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TreeSitter;

public class FileEditorView : Panel
{
    internal readonly FileEditorInput Input;
    public readonly TextEditor Editor;

    private TextMate.Installation textMateInstallation;
    private TextMateSharp.Grammars.Language Language;
    private RegistryOptions registryOptions;

    private Parser parser;
    private LanguageClient lspClient;
    private Process lspProcess;

    public FileEditorView(FileEditorInput input, RegistryOptions options)
    {
        Input = input;
        registryOptions = options;
        Focusable = true;
        Editor = new TextEditor
        {
            ShowLineNumbers = true,
            FontSize = 14,
            FontFamily = "Consolas",
            Text = input.TextContent,
            Focusable = true,
        };

        Editor.TextChanged += (s, e) =>
        {
            Input.UpdateContent(Editor.Text);
        };

        Editor.TextArea.TextEntered += (sender, e) =>
        {
            if (e.Text == "\n")
            {
                Caret caret = Editor.TextArea.Caret;
                IndentationManager.IndentAfterEnter("csharp", Editor.Document, caret.Line, 4, false);
            }
        };

        Editor.TextChanged += Editor_TextChanged;

        // InitializeLspAsync("javascript").GetAwaiter().GetResult();

        Children.Add(Editor);

        // parser = new Parser();
        // parser.Language = TreeSitter.JavaScript.JavaScriptLanguage.Create();

        LoadSyntaxHighlighting(options, input.FilePath);
    }

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        /*
        var tree = parser.Parse(Editor.Text);
        var root = tree.Root;

        Console.WriteLine("Tree-sitter AST root: " + root.Kind);

        foreach (var child in root.Children)
        {
            if (child.Kind == "function_declaration")
            {
                Console.WriteLine("Function found at line: " + child.StartPosition.Row);
            }
        }
        */
    }


    private async Task InitializeLspAsync(string languageId)
    {
        lspProcess = await LanguageToolManager.EnsureLanguageToolsAsync(languageId);

        // Connect OmniSharp client
        lspClient = LanguageClient.PreInit(options =>
        {
            options.WithInput(lspProcess.StandardOutput.BaseStream);
            options.WithOutput(lspProcess.StandardInput.BaseStream);
        });
        await lspClient.Initialize(new CancellationToken());

    }

    public void OnClosed()
    {
        lspProcess?.Kill();
        lspClient?.Dispose();
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

    public void ApplyTheme(RegistryOptions registryOptions)
    {
        if (Language == null) return;

        textMateInstallation = Editor.InstallTextMate(registryOptions);
        string scope = registryOptions.GetScopeByLanguageId(Language.Id);
        textMateInstallation.SetGrammar(scope);

        this.registryOptions = registryOptions;
    }

    public void OnConfigChanged()
    {
        object fontFamily = Application.Current.Resources["editor.font"];
        Editor.FontFamily = fontFamily == null ? "Consolas" : fontFamily.ToString();

        object fontSize = Application.Current.Resources["editor.fontsize"];
        Editor.FontSize = fontSize == null ? 14 : Convert.ToDouble(fontSize);
    }

    public void UpdateSettings()
    {
        Brush background = Application.Current.Resources.GetResource("editor.background");
        Editor.Background = background == null ? "#1f1f1f".GetColoredBrush() : background.ToString().GetColoredBrush();

        Editor.Foreground = Application.Current.Resources.GetResource("editor.foreground");

        Editor.WordWrap = MainWindow.EditorConfigsSettingsManager.Current.Editor.WordWrap;
    }

}
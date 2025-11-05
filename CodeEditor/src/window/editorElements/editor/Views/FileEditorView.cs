

using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.TextMate;
using Microsoft.Extensions.Logging.Abstractions;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TreeSitter;

public class FileEditorView : Panel
{
    internal readonly FileEditorInput Input;
    public readonly TextEditor Editor;
    public readonly Stream fileStream;

    private TextMate.Installation textMateInstallation;
    private TextMateSharp.Grammars.Language Language;
    private RegistryOptions registryOptions;

    private ILanguageClient languageClient;

    private CompletionWindow completionWindow;

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

        fileStream = File.Open(input.FilePath, FileMode.OpenOrCreate);

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
        Editor.TextChanged += async (_, _) => await OnEditorTextChangedAsync();
        Editor.TextArea.TextEntered += OnTextEntered;
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

        Children.Add(Editor);

        LoadSyntaxHighlighting(options, input.FilePath);

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        InitializeLspAsync(Language.Id).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    private async Task OnEditorTextChangedAsync()
    {
        Input.UpdateContent(Editor.Text);

        if (languageClient?.TextDocument == null)
            return;

        try
        {
            languageClient.TextDocument.DidChangeTextDocument(new DidChangeTextDocumentParams
            {
                TextDocument = new OptionalVersionedTextDocumentIdentifier
                {
                    Uri = new Uri(Input.FilePath),
                    Version = 1
                },
                ContentChanges = new Container<TextDocumentContentChangeEvent>(
                    new TextDocumentContentChangeEvent { Text = Editor.Text })
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LSP Sync Error] {ex.Message}");
        }
    }

    private async Task InitializeLspAsync(string languageId)
    {
        try
        {
            var serverPath = "omnisharp";
            if (OperatingSystem.IsWindows())
            {
                // fallback: try typical OmniSharp install directory
                var altPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "OmniSharp", "omnisharp.exe");
                if (File.Exists(altPath))
                    serverPath = altPath;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = serverPath,
                    Arguments = "-lsp",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();

            languageClient = await LanguageClient.From(options =>
                options
                    .WithInput(process.StandardOutput.BaseStream)
                    .WithOutput(process.StandardInput.BaseStream)
                    .WithLoggerFactory(new NullLoggerFactory())
            );

            // Register this file in LSP session
            var openParams = new DidOpenTextDocumentParams
            {
                TextDocument = new TextDocumentItem
                {
                    Uri = new Uri(Input.FilePath),
                    LanguageId = languageId,
                    Text = Editor.Text,
                    Version = 1
                }
            };

            languageClient.TextDocument.DidOpenTextDocument(openParams);

            Console.WriteLine("✅ OmniSharp LSP initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ LSP init failed: {ex.Message}");
        }
    }

    private async void OnTextEntered(object sender, TextInputEventArgs e)
    {
        if (e.Text == "\n")
        {
            Caret caret = Editor.TextArea.Caret;
            IndentationManager.IndentAfterEnter("csharp", Editor.Document, caret.Line, 4, false);
        }
        if (languageClient == null)
            return;

        if (!char.IsLetterOrDigit(e.Text.First()) && e.Text != ".")
            return;

        try
        {
            var caret = Editor.TextArea.Caret;
            var position = new Position(caret.Line - 1, caret.Column - 1);

            var completion = await languageClient.RequestCompletion(
                new CompletionParams
                {
                    TextDocument = new TextDocumentIdentifier(new Uri(Input.FilePath)),
                    Position = position
                });

            if (completion?.Items == null || completion.Items.Count() == 0)
                return;

            completionWindow = new CompletionWindow(Editor.TextArea)
            {
                CloseWhenCaretAtBeginning = true
            };

            var data = completionWindow.CompletionList.CompletionData;
            foreach (var item in completion.Items)
                data.Add(new SimpleCompletionData(item.Label));

            completionWindow.Show();
            completionWindow.Closed += (_, __) => completionWindow = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LSP Completion Error] {ex.Message}");
        }
    }

    public void OnClosed()
    {
        fileStream.Close();
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

    public void Save()
    {
        using var writer = new StreamWriter(fileStream, Editor.Encoding, leaveOpen: true);
        writer.Write(Input.TextContent);
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

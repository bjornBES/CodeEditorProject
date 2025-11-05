using System;
using Avalonia;
using lib.debug;
using CodeEditor;

public class Program
{
    public const string AppName = "CodeEditorApp";
    public static PipeServer PipeServer;
    public static int Main(string[] args)
    {
        DebugWriter.Clean();
        DebugWriter.Initialize(Console.Out);
        DebugWriter.AddModule("Main", "log_Main_console", "Main");
        DebugWriter.AddModulesToLog("Main", "Main.API", "Commands", "KeybindingManager", "Window", "Explorer", "Top palette", "Editor", "Side panel", "Config");
        DebugWriter.AddModule("AvaloniaEdit", "log_AvaloniaEdit_console", "AvaloniaEdit");
        Thread serverThread = new Thread(new ThreadStart(StartServer));
        serverThread.Name = "Server Thread";
        serverThread.Start();
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
        
    private static void StartServer()
    {
        PipeServer = new PipeServer("extensionPipe");
        PipeServer.OnPackageReceived += ExtensionManager.PackageReceived;
        using CancellationTokenSource cts = new CancellationTokenSource();

        _ = PipeServer.StartAsync(cts.Token);
    }
	
    static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .LogToTrace();
}
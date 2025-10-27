
using System.Text;
namespace lib.debug;

public static class DebugWriter
{
    private static TextWriter originalWriter;

    private static Dictionary<string, string> AltLogFileName = new Dictionary<string, string>();
    private static Dictionary<string, string> AltLogModule = new Dictionary<string, string>();

    public static void Initialize(TextWriter writer)
    {
#if DEBUG
        Directory.CreateDirectory("logs");
        originalWriter = writer;
        Console.SetOut(new InterceptWriter(writer));
#endif
    }

    public static void AddModule(string module, string FileName, string logModuleDirectory)
    {
        AltLogFileName.Add(module, FileName);
        AltLogModule.Add(module, logModuleDirectory);
    }

    public static void AddModules(string FileName, string logModuleDirectory, params string[] modules)
    {
        foreach (string module in modules)
        {
            AltLogFileName.Add(module, FileName);
            AltLogModule.Add(module, logModuleDirectory);
        }
    }

    public static void AddModuleToLog(string module, string FileName, string logModuleDirectory)
    {
        AltLogFileName.Add(module, FileName);
        AltLogModule.Add(module, logModuleDirectory);
    }

    public static void AddModulesToLog(string parentModule, params string[] modules)
    {
        string FileName = AltLogFileName[parentModule];
        string logModuleDirectory = AltLogModule[parentModule];

        foreach (string module in modules)
        {
            AltLogFileName.Add(module, FileName);
            AltLogModule.Add(module, logModuleDirectory);
        }
    }

    public static void Clean()
    {
        if (Directory.Exists("logs"))
        {
            Directory.GetFiles("logs", "debug*.log", SearchOption.AllDirectories).ToList().ForEach(File.Delete);
            File.Delete("logs/debug.log");

            Directory.EnumerateDirectories("logs", "*", SearchOption.AllDirectories).ToList().ForEach(Directory.Delete);
        }
    }
#nullable enable
    public static void WriteLine(string module, string? value)
    {
#if DEBUG
        originalWriter.WriteLine($@"[{module}] {DateTime.Now:HH:mm:ss} {value}");
        AddToLogFile(module, nameof(WriteLine), value + Environment.NewLine);
        // Additionally log to a debug log file or system if needed
#else
        Console.WriteLine($@"[{module}] {DateTime.Now:HH:mm:ss} {value}");
#endif
    }

    public static void Write(string module, string? value)
    {
#if DEBUG
        originalWriter.Write($@"[{module}] {DateTime.Now:HH:mm:ss} {value}");
        AddToLogFile(module, nameof(Write), value + Environment.NewLine);
        // Additionally log to a debug log file or system if needed
#else
        Console.Write($@"[{module}] {DateTime.Now:HH:mm:ss} {value}");
#endif
    }

    static void AddToLogFile(string module, string func, string? value)
    {
#if DEBUG
        string LogFileName = AltLogFileName[module];
        string LogModule = AltLogModule[module];
        if (!string.IsNullOrEmpty(LogFileName))
        {
            if (!Directory.Exists($"logs/{LogModule}"))
            {
                Directory.CreateDirectory($"logs/{LogModule}");
            }
            File.AppendAllText($"logs/{LogModule}/debug_{LogFileName}.log", $"{module} STDOUT {func} {DateTime.Now:HH:mm:ss} {value}", Encoding.UTF8);
            File.AppendAllText($"logs/debug_log_all_console.log", $"{module} STDOUT {func} {DateTime.Now:HH:mm:ss} {value}", Encoding.UTF8);
        }
#endif
    }
}
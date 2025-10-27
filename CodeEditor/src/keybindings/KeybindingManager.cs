using Avalonia;
using ReactiveUI;
using Avalonia.Controls;
using Avalonia.Input;
using lib.debug;
using System.Reactive;
using DynamicData;
using System.Diagnostics;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;

public class keyBindingData
{
    public keyBindingData(Key key, KeyModifiers modifiers, string context)
    {
        Key = key;
        Modifiers = modifiers;
        Context = context;
    }
    public Key Key;
    public KeyModifiers Modifiers;
    public string Context;
}

public class KeybindingManager
{
    private static readonly Dictionary<keyBindingData, string> _bindings = new();

    private static string _activeContext { get; set; } = "global";
    public static string ActiveContext { get { return _activeContext; } set { _activeContext = value; DebugWriter.WriteLine("KeybindingManager", $"Context switched {value}"); } }

    public static MainWindow mainWindow { get; set; }

    public static void BindKey(Key key, KeyModifiers modifiers, string context, string commandId)
    {
        _bindings[new keyBindingData(key, modifiers, context)] = commandId;

        ReactiveCommand<string, Unit> command;
        KeyGesture gesture = new KeyGesture(key, modifiers);
        command = ReactiveCommand.Create<string>(PerformCommand);
        KeyBinding keyBinding = new KeyBinding() { Gesture = gesture, Command = command, CommandParameter = $"{commandId},{context},{gesture}" };
        mainWindow.KeyBindings.Add(keyBinding);
    }

    public static void PerformCommand(string data)
    {
        string commandId = data.Split(',')[0];
        string context = data.Split(',')[1];
        KeyGesture gesture = KeyGesture.Parse(data.Split(',')[2]);

#nullable enable
        keyBindingData? other = null;
#nullable disable

        for (int i = 0; i < _bindings.Count; i++)
        {
            keyBindingData keyBindingOther = _bindings.Keys.ToArray()[i];
            KeyGesture otherkeyGesture = new KeyGesture(keyBindingOther.Key, keyBindingOther.Modifiers);
            if (gesture.Equals(otherkeyGesture) && !context.Equals(keyBindingOther.Context, StringComparison.OrdinalIgnoreCase))
            {
                other = keyBindingOther;
                break;
            }
        }

        bool result = ExecuteKeybinding(commandId, context);
        if (result == false && other != null)
        {
            result = ExecuteKeybinding(_bindings[other], other.Context);
        }
    }

    static bool ExecuteKeybinding(string commandId, string context)
    {
        CommandEntry commandEntry = CommandManager.GetCommandEntry(commandId);
        if (commandEntry == null)
        {
            DebugWriter.WriteLine("KeybindingManager", $"command Entry is null from {commandId}");
            throw new ArgumentNullException("commandEntry");
        }

        bool result = false;
        if (context == "global")
        {
            result = true;
        }
        else
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            // 1. get the properties

            List<string> namesOfProperties = new List<string>();
            {
                string[] segments = context.Split("&|".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                // ["editor", "!activeEditorIsPinned", "editorLangId", "==", "'csharp'"]
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i] == "==")
                    {
                        string value = segments[i + 1];
                        if (ControlElementData.ContextKeys.ContainsKey(value))
                        {
                            segments.Remove([segments[i]]);
                            i--;
                        }
                        else
                        {
                            segments.Remove([segments[i], segments[i + 1]]);
                            i -= 2;
                        }
                    }
                    segments[i] = segments[i].Trim("!()".ToCharArray());
                }
                // ["editor", "activeEditorIsPinned", "editorLangId"]
                namesOfProperties.Add(segments);
            }

            // 2. get the values from the properties
            List<PropertyData> properties = new List<PropertyData>();
            foreach (string name in namesOfProperties)
            {
                if (ControlElementData.ContextKeys.ContainsKey(name))
                {
                    properties.Add(ControlElementData.ContextKeys[name]);
                }
            }

            // 3. make the c# code
            string code = "";
            string copyContext = new string(context);
            foreach (PropertyData property in properties)
            {
                if (property == null)
                {
                    return false;
                }

                copyContext = copyContext.Replace(property.GetName(), property.GetValue().ToString().ToLower());
            }

            code += $"System.Convert.ToInt32({copyContext})";

            object _result = CSharpScript.EvaluateAsync(code).GetAwaiter().GetResult();
            // 5. get return value (true or false)
            result = Convert.ToBoolean(_result);
            DebugWriter.WriteLine("KeybindingManager", $"Result = {result}");
            stopwatch.Stop();
            DebugWriter.WriteLine("KeybindingManager", $"Time = {stopwatch.ElapsedMilliseconds}ms");
        }

        if (result)
        {
            CommandManager.ExecuteCommandGetArgs(commandId);
        }
        return result;
    }


    // Convenience: attach manager to a Window (wires KeyDown/KeyUp and deactivation)
    public static void AttachToWindow(MainWindow window)
    {
        mainWindow = window;
    }
}

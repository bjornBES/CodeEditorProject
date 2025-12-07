
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using GlobalLibrary;
using lib.debug;

public static class CommandManager
{
    public static List<CommandEntry> commandEntries = new List<CommandEntry>();


    public static void RegisterCommand(string commandName, string commandId, Delegate callback)
    {
        commandEntries.Add(new CommandEntry(commandName, commandId, callback));
    }

    public static void RegisterExtensionCommand(string commandName, string commandId, string clientId, Type[] argTypes)
    {
        commandEntries.Add(new CommandEntry(commandName, commandId, clientId, argTypes));
    }

    public static CommandEntry GetCommandEntry(string commandId)
    {
        return commandEntries.FirstOrDefault(x => x.CommandId == commandId);
    }

    public static void UpdateCommandEntry(string commandId, Delegate callback)
    {
        CommandEntry commandEntry = GetCommandEntry(commandId);
        commandEntries.Remove(commandEntry);
        RegisterCommand(commandEntry.DisplayName, commandId, callback);
    }

    public static object ExecuteCommandGetArgs(string commandId)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        CommandEntry entry = null;

        Stopwatch seachList = Stopwatch.StartNew();
        foreach (CommandEntry k in commandEntries)
        {
            if (k.CommandId == commandId)
            {
                entry = k;
                break;
            }
        }

        List<object> args = new List<object>();

        DebugWriter.WriteLine("Commands", $"seachList: Elapsed time {seachList.ElapsedMilliseconds} ms commandID = {commandId}");
        seachList.Stop();
        return RunCommand(entry, commandId, args.ToArray());
    }

    public static object ExecuteCommand(string commandId, params object[] args)
    {
        CommandEntry entry = null;
        /*
                if (!(args == null || args.Length == 0))
                {
                    if ((args != null || args.Length == 0) && args[0] is JsonElement)
                    {
                        FromJsonElement(ref args);
                    }
                }
        */
        Stopwatch seachList = Stopwatch.StartNew();
        foreach (CommandEntry k in commandEntries)
        {
            if (k.CommandId == commandId)
            {
                if (k.Types.Length == args.Length)
                {
                    if (k.Types.Zip(args, (expected, actual) => expected.IsAssignableFrom(actual.GetType())).All(match => match))
                    {
                        entry = k;
                        break;
                    }
                }
            }
        }
        DebugWriter.WriteLine("Commands", $"seachList: Elapsed time {seachList.ElapsedMilliseconds} ms commandID = {commandId}");
        seachList.Stop();
        return RunCommand(entry, commandId, args);
    }

    public static object RunCommand(CommandEntry entry, string commandId, params object[] args)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        if (entry == null)
        {
            return null;
        }

        if (entry.IsClientCommand)
        {
            DebugWriter.WriteLine("Commands", $"Running client command '{entry.CommandId}' for client '{entry.ClientId}'");
            CommandPackage commandPackage = new CommandPackage()
            {
                CommandId = entry.CommandId,
                CommandName = entry.DisplayName,
                CommandArgs = args
            };
            ExtensionManager.SendPackage(entry.ClientId, PackageTypes.Command, commandPackage);

            return null; // Or some appropriate response
        }
        object returnResult = entry.Callback.DynamicInvoke(args);

        if (commandId.StartsWith("editor"))
        {
        }
        stopwatch.Stop();
        DebugWriter.WriteLine("Commands", $"ExecuteCommand: Elapsed time {stopwatch.ElapsedMilliseconds} ms commandID = {commandId}");
        return returnResult;
    }
}
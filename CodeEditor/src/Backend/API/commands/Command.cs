[Serializable]
public class CommandEntry
{
    public string DisplayName { get; set; }
    public string CommandId { get; set; }
    public Delegate Callback { get; set; }
    public string ClientId { get; set; }
    public Type[] Types { get; set; }
    public bool ShowInCC { get; set; }
    public bool IsClientCommand { get; set; } = false;

    public CommandEntry(string name, string id, Delegate callback, bool showInCC = true)
    {
        ShowInCC = showInCC;
        DisplayName = name;
        CommandId = id;
        Callback = callback;
        Type[] paramTypes = callback.Method.GetParameters().Select(p => p.ParameterType).ToArray();
        Types = paramTypes;
    }
    public CommandEntry(string name, string id, string clientId, Type[] types, bool showInCC = true)
    {
        ShowInCC = showInCC;
        DisplayName = name;
        CommandId = id;
        ClientId = clientId;
        IsClientCommand = true;
        Types = types;
    }
}
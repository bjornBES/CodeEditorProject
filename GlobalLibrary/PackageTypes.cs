
namespace GlobalLibrary;

public enum PackageTypes : int
{
    // For raw string packages
    Message,
    // For running commands
    Command,
    // For getting the return value from a command
    CommandReturn,
    // For adding features to the editor
    Addon,
    // For responding to a package
    Response,
}

public class PackageId
{
    static Dictionary<PackageTypes, string> _packageIds = new Dictionary<PackageTypes, string>()
    {
        { PackageTypes.Message, "msg"},
        { PackageTypes.Command, "cmd"},
        { PackageTypes.CommandReturn, "retcmd"},
        { PackageTypes.Addon, "addon"},
        { PackageTypes.Response, "resp"},
    };
    public static string PackageTypeToId(PackageTypes packageTypes)
    {
        return _packageIds[packageTypes];
    }
    public static PackageTypes FromIdToPackageType(string packageType)
    {
        foreach (KeyValuePair<PackageTypes, string> kvp in _packageIds)
        {
            if (kvp.Value == packageType)
                return kvp.Key;
        }
        return PackageTypes.Message;
    }
}
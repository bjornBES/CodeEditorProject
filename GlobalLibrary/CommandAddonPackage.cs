namespace GlobalLibrary;

public class CommandAddonPackage : AddonPackageData
{
    public string CommandId { get; set; }
    public string CommandName { get; set; }
    public string[] CommandArgTypes { get; set; }
}
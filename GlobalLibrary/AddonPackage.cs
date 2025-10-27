namespace GlobalLibrary;

public class AddonPackage
{
    public AddonPackageTypes addonPackageType { get; set; }
    public string addonPackageData { get; set; }
}

public enum AddonPackageTypes
{
    // CommandAddonPackage
    command,
}

public class AddonPackageData
{

}
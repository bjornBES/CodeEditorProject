namespace GlobalLibrary;

public class Package
{
    public required string ClientId { get; set; }
    public required string PackageId { get; set; }
    public required int PackageSize { get; set; }
    public required byte[] PackageData { get; set; }
}
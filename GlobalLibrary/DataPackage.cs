
namespace GlobalLibrary;
#nullable disable
public class DataPackage
{
    public string ClientId { get; set; }
    public string PackageId { get; set; }
    public int PackageSize { get; set; }
    public byte[] PackageData { get; set; }
}
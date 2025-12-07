
public class GlobalStorageSettings : Settings<GlobalStorageSettings>
{
    public string DefaultTheme { get; set; }
    public List<string> RecentWorkspaces { get; set; } = new List<string>();
    public List<string> RecentFolders { get; set; } = new List<string>();
    /*
    Item11
    Item1
    Item10
    Item9
    Item8
    Item7
    Item6
    Item5
    Item4
    Item3
    */
    public List<string> RecentFiles { get; set; } = new List<string>();
}
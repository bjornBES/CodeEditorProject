public class ExplorerNode
{
    public string Header { get; set; }
    public string Path { get; set; }
    public List<ExplorerNode> Children { get; set; } = new List<ExplorerNode>();
    public bool IsDirectory { get; set; }
}

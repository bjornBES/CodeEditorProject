
public class Workspace
{
    public string Name { get; set; }
    public string Path { get; set; }
    public int Order { get; set; }

    public Workspace(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public override string ToString()
    {
        return $"{{ Name: {Name}, Path: {Path} }}";
    }
}

public class Document
{
    public string Name { get; set; }
    public Uri URI { get; set;}

    public Document(string path)
    {
        Name = Path.GetFileName(path);
        URI = new Uri(path);
    }

    public override string ToString()
    {
        return $"{{ Name = {Name}, URI={URI} }}";
    }
}
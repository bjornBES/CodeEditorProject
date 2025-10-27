
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace CodeEditor;
public static class Resource
{
    private static ResourceManager resourceManager;
    private static CultureInfo resourceCulture;

    public static ResourceManager ResourceManager
    {
        get
        {
            if (ReferenceEquals(resourceManager, null))
            {
                // Could not find the resource "CodeEditor.Resources.resources" among the resources "CodeEditor.Resource.resources" what the fu*k Microsoft?
                resourceManager = new ResourceManager("CodeEditor.Resource", typeof(Resource).Assembly);
            }
            return resourceManager;
        }
    }

    public static CultureInfo Culture
    {
        get => resourceCulture;
        set => resourceCulture = value;
    }

    public static byte[] GetImage(string path)
    {
        object obj = ResourceManager.GetObject(path, resourceCulture);
        return ((byte[])(obj));
    }

    public static byte[] ExplorerIcon
    {
        get
        {
            object obj = ResourceManager.GetObject("explorerIcon", resourceCulture);
            return ((byte[])(obj));
        }
    }
}
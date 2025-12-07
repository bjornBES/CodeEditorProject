
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Extensions;
using GlobalLibrary;

public static class ExtensionManager
{
    public static readonly Dictionary<string, Extension> LoadedExtensions = new();

    public static void LoadExtensions()
    {
        string folder = AppPaths.ExtensionsDirectoryPath;
        string[] directories = Directory.GetDirectories(folder);
        foreach (string d in directories)
        {
            string[] jsonFiles = Directory.GetFiles(d, "*.json", SearchOption.TopDirectoryOnly);
            string manifestPath = jsonFiles.First();
            string json = File.ReadAllText(manifestPath);
            ExtensionManifest manifest = JsonSerializer.Deserialize<ExtensionManifest>(json, new JsonSerializerOptions() { NewLine = "\n", WriteIndented = true });
            Register(manifest, manifestPath);
            ActivateExtension(manifest);
        }
    }

    public static void Register(ExtensionManifest manifest, string manifestPath)
    {
        try
        {
            string extensionPath = Path.GetDirectoryName(manifestPath)!;
            string assemblyPath = Path.Combine(extensionPath, manifest.Main!);
            Assembly assembly = Assembly.LoadFrom(assemblyPath);

            Type type = assembly.GetType(manifest.Entry!)!;
            Extension instance = (Extension)Activator.CreateInstance(type)!;
            instance.StartExtension(instance, manifest, new[] { extensionPath });

            LoadedExtensions.Add(manifest.Name, instance);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExtensionManager] Failed to load {manifest?.Name}: {ex.Message}");
        }
    }

    public static void ActivateExtension(ExtensionManifest manifest)
    {
        try
        {
            Extension extension = LoadedExtensions[manifest.Name];
            extension.Activate();

            foreach (ExtensionContributesCommands commands in manifest.Contributes.Commands)
            {
                // CommandManager.RegisterExtensionCommand(commands.Title, commands.Command, manifest.Name, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExtensionManager] Failed to load {manifest?.Name}: {ex.Message}");
        }
    }

    public static void DeactivateAll()
    {
        foreach (Extension ext in LoadedExtensions.Values)
        {
            try
            {
                ClientConnection clientConnection = Program.PipeServer._clients[ext.clientId];
                clientConnection.SendLine("exit -");
                ext.Stop();
            }
            catch (Exception ex) { Console.WriteLine($"Error deactivating {ext}: {ex.Message}"); }
        }
        LoadedExtensions.Clear();
    }

    public static void SendPackage<T>(string clientId, PackageTypes packageTypes, T dataPackage)
    {
        if (!LoadedExtensions.TryGetValue(clientId, out Extension extension))
        {
            // Not loaded
        }

        string json = JsonSerializer.Serialize(dataPackage);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        Package package = new Package() { ClientId = "AS:SERVER", PackageId = PackageId.PackageTypeToId(packageTypes), PackageSize = json.Length, PackageData = bytes };

        extension.OnReceivePackage.Invoke(package);
    }

    public static void PackageReceived(Package package)
    {
        PackageTypes packageId = PackageId.FromIdToPackageType(package.PackageId);
        switch (packageId)
        {
            case PackageTypes.Addon:
                string json = Encoding.UTF8.GetString(package.PackageData);
                AddonPackage addonPackage = JsonSerializer.Deserialize<AddonPackage>(json);
                AddonPackageReceived(addonPackage, package.ClientId);
                break;
        }
    }

    public static void AddonPackageReceived(AddonPackage package, string clientId)
    {
        switch (package.addonPackageType)
        {
            case AddonPackageTypes.command:
                CommandAddonPackage commandAddon = JsonSerializer.Deserialize<CommandAddonPackage>(package.addonPackageData);
                List<Type> types = new List<Type>();
                foreach (string strType in commandAddon.CommandArgTypes)
                {
                    Type type = Type.GetType(strType);
                    types.Add(type);
                }
                CommandManager.RegisterExtensionCommand(commandAddon.CommandName, commandAddon.CommandId, clientId, types.ToArray());
                break;
        }
    }
}
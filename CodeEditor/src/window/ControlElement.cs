
using System.Reflection;
using Avalonia.Controls;

public class PropertyData
{
    public string PropertyName { get; set; }
    public PropertyInfo PropertyInfo { get; set; }
    public Type InstanceType { get; set; }
    public object Instance { get; set; }

    public string GetName()
    {
        return PropertyName;
    }

    public string GetTypeAsString()
    {
        if (InstanceType == null)
        {
            return null;
        }

        return PropertyInfo.PropertyType.FullName;
    }
    public object GetValue()
    {
        return PropertyInfo.GetValue(Instance);
    }
}

public static class ControlElementData
{
    public static Dictionary<string, PropertyData> ContextKeys { get; set; } = new Dictionary<string, PropertyData>();
}

public class ControlElement<T> : Panel
{

    public void Initialize()
    {
        Focusable = true;
    }


    public void AddContext(string key, PropertyInfo propertyInfo, T objInstance)
    {
        if (ControlElementData.ContextKeys.ContainsKey(key))
        {
            return;
        }
        ControlElementData.ContextKeys.Add(key, new PropertyData() { Instance = objInstance, InstanceType = typeof(T), PropertyInfo = propertyInfo, PropertyName = key });
    }

    public void UpdateInstance(string key, T instance)
    {
        ControlElementData.ContextKeys[key].Instance = instance;
    }

    public void UpdateInstance<T1>(string key, T1 instance)
    {
        if (!ControlElementData.ContextKeys.ContainsKey(key))
        {
            return;
        }
        if (ControlElementData.ContextKeys[key].InstanceType != typeof(T1))
        {
            return;
        }
        ControlElementData.ContextKeys[key].Instance = instance;
    }

    public PropertyInfo GetPropertyInfo(string name)
    {
        return typeof(T).GetProperty(name);
    }
}

public class Element<T>
{

    public void Initialize()
    {
    }


    public void AddContext(string key, PropertyInfo propertyInfo, T objInstance)
    {
        if (ControlElementData.ContextKeys.ContainsKey(key))
        {
            return;
        }
        ControlElementData.ContextKeys.Add(key, new PropertyData() { Instance = objInstance, InstanceType = typeof(T), PropertyInfo = propertyInfo, PropertyName = key });
    }

    public void UpdateInstance(string key, T instance)
    {
        ControlElementData.ContextKeys[key].Instance = instance;
    }

    public void UpdateInstance<T1>(string key, T1 instance)
    {
        if (!ControlElementData.ContextKeys.ContainsKey(key))
        {
            return;
        }
        if (ControlElementData.ContextKeys[key].InstanceType != typeof(T1))
        {
            return;
        }
        ControlElementData.ContextKeys[key].Instance = instance;
    }

    public PropertyInfo GetPropertyInfo(string name)
    {
        return typeof(T).GetProperty(name);
    }
}
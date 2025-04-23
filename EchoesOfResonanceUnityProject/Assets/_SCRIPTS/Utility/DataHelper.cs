using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public static class DH
{
    private static readonly Dictionary<Type, ScriptableObject> _registry = new Dictionary<Type, ScriptableObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void RegisterAllAtStartup()
    {
        GlobalData[] allData = Resources.LoadAll<GlobalData>("");
        foreach (var data in allData)
        {
            Register(data.GetType(), data);
        }
    }
    public static void Register(Type type, ScriptableObject instance)
    {
        if (!_registry.ContainsKey(type))
        {
            _registry.Add(type, instance);
        }
    }

    public static T Get<T>() where T : ScriptableObject
    {
        var type = typeof(T);
        if (_registry.TryGetValue(type, out var instance))
        {
            return instance as T;
        }

        Debug.LogError($"Global data of type {type.Name} is not registered.");
        return null;
    }
}

using UnityEngine;
using UnityEditor;
using System.Linq;

public static class DataHelper
{
    public static T[] GetDataOfType<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        return guids.Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
    }
}

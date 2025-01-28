using UnityEngine;
using UnityEditor;
using System.Linq;

public static class DataHelper
{
    public static T[] GetDataOfType<T>() where T : ScriptableObject
    {
        return Resources.LoadAll<T>("").ToArray();
    }
}

using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/Global/GlobalGemData", order = 0)]
public class GlobalGemData : ScriptableObject
{

    private static GlobalGemData _root;

    public static GlobalGemData root
    {
        get
        {
            if (_root == null)
            {
                _root = DataHelper.GetDataOfType<GlobalGemData>()[0];
                if (_root == null)
                {
                    Debug.LogError("TestOverrides asset not found in Resources!");
                }
            }
            return _root;
        }
    }
    public Mesh[] gemMeshes;
    public Color[] gemColors;
}

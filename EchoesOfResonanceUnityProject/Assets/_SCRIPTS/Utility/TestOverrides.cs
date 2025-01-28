using NUnit.Framework.Internal;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Utility/TestOverrides", order = 0)]
public class TestOverrides : ScriptableObject
{
    private static TestOverrides _root;

    public static TestOverrides root
    {
        get
        {
            if (_root == null)
            {
                _root = DataHelper.GetDataOfType<TestOverrides>()[0];
                if (_root == null)
                {
                    Debug.LogError("TestOverrides asset not found in Resources!");
                }
            }
            return _root;
        }
    }
    public bool skipIntro;
    public bool ignoreMidi;
}

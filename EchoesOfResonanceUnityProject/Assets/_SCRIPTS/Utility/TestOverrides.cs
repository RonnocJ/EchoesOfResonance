using NUnit.Framework.Internal;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Utility/TestOverrides", order = 0)]
public class TestOverrides : GlobalData
{
    public bool skipIntro;
    public bool ignoreMidi;
    [Range(0.01f, 1)]
    public float uiSpeed;
}

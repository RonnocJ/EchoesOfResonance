using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/Global/GlobalGemData", order = 0)]
public class GlobalGemData : GlobalData
{
    [Serializable]
    public class GemColors
    {
        [ColorUsage(true, true)]
        public Color mainColor;
        [ColorUsage(true, true)]
        public Color bottomColor;
        [ColorUsage(true, true)]
        public Color topColor;
    }
    public Mesh[] gemMeshes;
    public GemColors[] gemColors;
}

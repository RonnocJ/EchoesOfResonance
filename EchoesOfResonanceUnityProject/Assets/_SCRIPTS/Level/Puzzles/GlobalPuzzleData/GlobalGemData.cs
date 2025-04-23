using System;
using System.Collections.Generic;
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
    public GemColors[] gemColors;
    public readonly Dictionary<int, int> gemMeshIndicies = new()
    { {0, 3}, {1, 0}, {2, 1}, {3, -1}, {4, 2} };
}

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/DoorData", order = 0)]
public class DoorData : ScriptableObject
{
    [Serializable]
    public class SolutionData
    {
        public string correctNote;
        public Color gemColor;
        [HideInInspector]
        public bool noteHeld;
    }
    public SolutionData[] solutions;
    public int solved;
}

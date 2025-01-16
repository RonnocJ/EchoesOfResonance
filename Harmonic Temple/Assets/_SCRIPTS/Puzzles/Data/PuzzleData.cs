using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/PuzzleData", order = 0)]
public class PuzzleData : ScriptableObject
{
    public PuzzleType puzzleType;
    [Serializable]
    public class SolutionData
    {
        public string correctNote;
        public TrData gemTransform;
    }
    public SolutionData[] solutions;
    public int solved;
}

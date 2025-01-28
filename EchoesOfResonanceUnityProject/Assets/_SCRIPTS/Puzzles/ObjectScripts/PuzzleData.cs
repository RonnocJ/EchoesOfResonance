using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/PuzzleData", order = 0)]
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
    public bool hasOrphans;
    [SerializeField] private int _solved;
    public int solved
    {
        get => _solved;
        set
        {
            if (_solved != value) OnValueChanged.Invoke(_solved);
            _solved = value;
            if (value == solutions.Length) OnPuzzleCompleted.Invoke();
        }
    }

    public Action<int> OnValueChanged;
    public Action OnPuzzleCompleted;
}

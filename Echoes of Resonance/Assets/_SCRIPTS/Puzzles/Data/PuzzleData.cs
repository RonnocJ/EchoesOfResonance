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
    public bool hasOrphans;
    [SerializeField] private int _solved;
    public int solved
    {
        get => _solved;
        set
        {
            _solved = value;
            OnValueChanged?.Invoke(_solved);
            if(value == solutions.Length)
            {
                OnPuzzleCompleted.Invoke();
            }
            
        }
    }

    public delegate void ValueChanged(int newValue);
    public event ValueChanged OnValueChanged;
    public Action OnPuzzleCompleted;
}

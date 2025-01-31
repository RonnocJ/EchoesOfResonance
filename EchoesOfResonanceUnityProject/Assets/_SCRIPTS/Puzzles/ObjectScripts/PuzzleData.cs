using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/PuzzleData", order = 0)]
public class PuzzleData : ScriptableObject
{
    public PuzzleType puzzleType;
    public string[] solutions;
    [SerializeField] private int _solved;
    public int solved
    {
        get => _solved;
        set
        {
            if (_solved != value) OnSolvedChanged.Invoke(value);
            _solved = value;
            if (value == solutions.Length) OnPuzzleCompleted.Invoke();
        }
    }
    public AK.Wwise.Trigger onCompleteStinger;
    public AK.Wwise.State onCompleteState;
    [SerializeField] private int _reset;
    public int reset
    {
        get => _reset;
        set
        {
            _reset = value;
            if (value == 3) OnReset.Invoke();
        }
    }

    public Action<int> OnSolvedChanged;
    public Action OnPuzzleCompleted;
    public Action OnReset;

    public void SetMusicComplete()
    {
        if(onCompleteStinger != null)
        {
            onCompleteStinger.Post(MusicManager.root.gameObject);
        }
        if(onCompleteState != null)
        {
            onCompleteState.SetValue();
        }
    }
}

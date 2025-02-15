using System;
using AK.Wwise;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/PuzzleData", order = 0)]
public class PuzzleData : ScriptableObject
{
    public PuzzleType puzzleType;
    public string[] solutions;
    public int[] checkpoints;
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
    public AudioEffects[] audioEffects;

    public Action<int> OnSolvedChanged;
    public Action OnPuzzleCompleted;
    public Action OnReset;

    public void SetMusicComplete()
    {
        if (audioEffects != null)
        {
            foreach (var effect in audioEffects)
            {
                if (effect.executeOnSolved)
                {
                    OnPuzzleCompleted += () => ExecuteActions(effect);
                }
                else
                {
                    OnSolvedChanged += c => { if (c == effect.executeEarly) ExecuteActions(effect); };
                }
            }
        }
    }

    public void ExecuteActions(AudioEffects effect)
    {
        if ((effect.audioTypes & AudioEffectType.Event) != 0)
        {
            foreach (var e in effect.audioEvents)
            {
                AudioManager.root.PlaySound(e, MusicManager.root.gameObject);
            }
        }
        if ((effect.audioTypes & AudioEffectType.State) != 0)
        {
            foreach (var st in effect.audioStates)
            {
                MusicManager.root.SetState(st);
            }
        }
        if ((effect.audioTypes & AudioEffectType.Switch) != 0)
        {
            foreach (var sw in effect.audioSwitches)
            {
                AudioManager.root.SetSwitch(sw, MusicManager.root.gameObject);
            }
        }
        if ((effect.audioTypes & AudioEffectType.Trigger) != 0)
        {
            foreach (var t in effect.audioTriggers)
            {
                MusicManager.root.SetTrigger(t);
            }
        }
        if ((effect.audioTypes & AudioEffectType.RTPC) != 0)
        {
            foreach (var r in effect.audioRTPCs)
            {
                AudioManager.root.SetRTPC(r.parameter, r.value);
            }
        }

    }
}

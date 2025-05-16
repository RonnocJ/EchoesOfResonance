using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class Broadcaster : MonoBehaviour, IInputScript
{
    private static List<Broadcaster> activeBroadcasters = new();
    public static GameObject obj;
    public static Action<float> OnBatteryChange;
    public static Action OnBatteryEmpty;
    public static Action OnBatteryCharge;
    private static float _batteryLevel = 1f;
    protected static float batteryLevel
    {
        get => _batteryLevel;
        set
        {
            if (_batteryLevel != value)
            {
                OnBatteryChange.Invoke(value - _batteryLevel);

                float clampedValue = Mathf.Clamp(value, 0, 1);

                if (value < 0)
                {
                    draining = false;
                    OnBatteryEmpty?.Invoke();
                }

                _batteryLevel = clampedValue;
            }
        }
    }
    public static bool draining;
    public static Action OnBatteryDrainChange;
    private static float _batteryDrainAmount;
    protected static float batteryDrainAmount
    {
        get => _batteryDrainAmount;
        set
        {
            if (_batteryDrainAmount != value)
            {
                if (value <= 0)
                {
                    draining = false;
                    _batteryDrainAmount = 0;
                }
                else
                {
                    draining = true;
                    _batteryDrainAmount = value;
                    OnBatteryDrainChange?.Invoke();
                }
            }
        }
    }
    public static float modInput;
    public static int finderLevel;
    public static PzNote finderEstimate;
    public static Action OnHeldNotesEmptied;
    public static HashSet<PzNote> heldNotes = new();
    public static PuzzleData activePuzzle;
    public static PuzzleStand activePlate;
    public virtual void Awake()
    {
        batteryLevel = 1;
        batteryDrainAmount = 0;
        finderEstimate = new PzNote(13);

        GameManager.root.OnStateChange += BrStateChange;
    }
    protected static void RegisterActiveBroadcaster(Broadcaster b)
    {
        if (!activeBroadcasters.Contains(b))
            activeBroadcasters.Add(b);
    }
    public void AddInputs()
    {
        InputManager.root.AddVelListener<int, int>(ActionTypes.KeyDown, MethodGate.Wrap<int, int>(this, nameof(OnNoteOn)));
        InputManager.root.AddListener<float>(ActionTypes.KeyUp, MethodGate.Wrap<float>(this, nameof(OnNoteOff)));
        InputManager.root.AddVelListener<int, int>(ActionTypes.ChordDown, MethodGate.Wrap<int, int>(this, nameof(OnChordOn)));
        InputManager.root.AddListener<float>(ActionTypes.ModwheelChange, MethodGate.Wrap<float>(this, nameof(ModChange)));
        InputManager.root.AddListener<float>(ActionTypes.Settings, MethodGate.Wrap<float>(this, nameof(SettingsChange)));
    }
    private void BrStateChange(GameState newState)
    {
        foreach (var br in activeBroadcasters)
        {
            switch (newState)
            {
                case GameState.Synced:
                    MethodGate.Wrap(br, nameof(OnPuzzleSynced))?.Invoke();
                    break;
                case GameState.InPuzzle:
                    MethodGate.Wrap(br, nameof(OnPuzzleEnter))?.Invoke();
                    break;
                case GameState.Roaming:
                    MethodGate.Wrap(br, nameof(OnPuzzleExit))?.Invoke();
                    break;
            }
        }
    }
    private static Action LockMethod(Broadcaster b, Action baseMethod)
    {
        return () =>
        {
            var actualType = b.GetType();
            var methodInfo = actualType.GetMethod(baseMethod.Method.Name);

            if (CheckStates(methodInfo)) return;

            methodInfo.Invoke(b, new object[] { });
        };
    }
    private static Action<float> LockMethod(Broadcaster b, Action<float> baseMethod)
    {
        return input1 =>
        {
            var actualType = b.GetType();
            var methodInfo = actualType.GetMethod(baseMethod.Method.Name);

            if (CheckStates(methodInfo)) return;

            methodInfo.Invoke(b, new object[] { input1 });
        };
    }
    private static Action<int, int> LockMethod(Broadcaster b, Action<int, int> baseMethod)
    {
        return (input1, input2) =>
        {
            var actualType = b.GetType();
            var methodInfo = actualType.GetMethod(baseMethod.Method.Name);

            if (CheckStates(methodInfo)) return;

            methodInfo.Invoke(b, new object[] { input1, input2 });
        };
    }
    private static bool CheckStates(MethodInfo methodInfo)
    {
        if (methodInfo == null || methodInfo.DeclaringType == typeof(Broadcaster)) return true;

        var currentState = GameManager.root.State;

        var allowed = (AllowedStates)Attribute.GetCustomAttribute(methodInfo, typeof(AllowedStates));
        var disallowed = (DissallowedStates)Attribute.GetCustomAttribute(methodInfo, typeof(DissallowedStates));
        var allAbove = (AllowAllAboveState)Attribute.GetCustomAttribute(methodInfo, typeof(AllowAllAboveState));

        if (allowed != null && !allowed.States.Contains(currentState)) return true;
        if (disallowed != null && disallowed.States.Contains(currentState)) return true;
        if (allAbove != null && currentState < allAbove.MinState) return true;

        return false;
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming, GameState.Synced)]
    public virtual void OnNoteOn(int newNote, int velocity)
    {
        heldNotes.Add(new PzNote(newNote));
    }
    public virtual void OnNoteOff(float oldNote)
    {
        heldNotes.Remove(new PzNote(oldNote));

        if (heldNotes.Count == 0)
        {
            OnHeldNotesEmptied?.Invoke();
        }
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming, GameState.Synced)]
    public virtual void OnChordOn(int newChord, int velocity) { }
    [AllowedStates(GameState.InPuzzle)]
    public virtual void ModChange(float newModInput)
    {
        modInput = newModInput;
    }
    [AllowAllAboveState(GameState.Cutscene), DissallowedStates(GameState.Shutdown)]
    public virtual void SettingsChange(float settingsInput) { }
    public virtual void OnPuzzleSynced() { }
    public virtual void OnPuzzleEnter() { }
    public virtual void OnPuzzleExit() { }
    public virtual void OnDestroy()
    {
        activeBroadcasters.Clear();
    }
}
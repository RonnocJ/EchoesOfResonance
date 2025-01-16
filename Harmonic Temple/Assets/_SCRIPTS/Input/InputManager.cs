using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Minis;
public enum ActionTypes
{
    KeyDown,
    KeyUp,
    ModwheelChange,
    PitchbendChange
}
[Serializable]
public class ActionData
{
    [HideInInspector]
    public object actionEvent;
    [HideInInspector]
    public float floatInput;
    public ActionData(object newAction)
    {
        actionEvent = newAction;
    }
}
public class InputManager : Singleton<InputManager>
{
    [HideInInspector]
    public bool usingKeyboard;
    private List<IInputScript> inputs = new List<IInputScript>();
    private Dictionary<ActionTypes, ActionData> actionDict = new Dictionary<ActionTypes, ActionData>();
    private MidiDevice midiController;
    private Melanchall.DryWetMidi.Multimedia.IInputDevice midiPitchController;

    void Start()
    {
        usingKeyboard = false;
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change != InputDeviceChange.Added) return;

            midiController = device as MidiDevice;
            midiPitchController = Melanchall.DryWetMidi.Multimedia.InputDevice.GetByIndex(0);
            midiPitchController.StartEventsListening();

            if (midiController == null) return;
            usingKeyboard = true;
            InitializeActions();
            ConfigureInputs();
        };

    }
    void InitializeActions()
    {
        actionDict.Add(ActionTypes.KeyDown, new ActionData(new Action<float>((_) => { })));
        actionDict.Add(ActionTypes.KeyUp, new ActionData(new Action<float>((_) => { })));
        actionDict.Add(ActionTypes.ModwheelChange, new ActionData(new Action<float>((_) => { })));
        actionDict.Add(ActionTypes.PitchbendChange, new ActionData(new Action<float>((_) => { })));

        midiController.onWillNoteOn += (note, velocity) =>
        {
            if (actionDict[ActionTypes.KeyDown].actionEvent is Action<float> method && GameManager.root.currentState != GameState.Shutdown)
            {
                method.Invoke(note.noteNumber);
            }
        };
        midiController.onWillNoteOff += note =>
        {
            if (actionDict[ActionTypes.KeyUp].actionEvent is Action<float> method && GameManager.root.currentState != GameState.Shutdown)
            {
                method.Invoke(note.noteNumber);
            }
        };
        midiController.onWillControlChange += (wheel, amount) =>
        {
            if (wheel.controlNumber == 1 && actionDict[ActionTypes.ModwheelChange].actionEvent is Action<float> method)
            {
                method.Invoke(amount);
            }
        };
        midiPitchController.EventReceived += (sender, message) =>
        {
            if (message.Event is Melanchall.DryWetMidi.Core.PitchBendEvent p && actionDict[ActionTypes.PitchbendChange].actionEvent is Action<float> method)
            {
                method((p.PitchValue - 8192f) / 8192f);
            }
        };
    }
    void ConfigureInputs()
    {
        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            if (behaviour.GetComponent<IInputScript>() != null && !inputs.Contains(behaviour.GetComponent<IInputScript>()))
            {
                inputs.Add(behaviour.GetComponent<IInputScript>());
            }
        }
        foreach (IInputScript input in inputs)
        {
            input.AddInputs();
        }
    }
    public void AddListener<T>(ActionTypes type, Action<T> method)
    {
        if (actionDict[type].actionEvent is Action<T> action)
        {
            actionDict[type].actionEvent = action + method;
        }
        else
        {
            Debug.LogError($"Couldn't add {method} of action type {type} as a listener within the input dictionary!");
        }
    }

    void OnDestroy()
    {
        if (midiController != null)
            InputSystem.RemoveDevice(midiController);
    }
}
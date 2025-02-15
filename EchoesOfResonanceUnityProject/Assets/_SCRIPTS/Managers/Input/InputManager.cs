using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using MidiDevice = Minis.MidiDevice;
using InputDevice = UnityEngine.InputSystem.InputDevice;
using System.Collections;

public enum ActionTypes
{
    KeyDown, KeyUp, ChordDown, ChordUp, ModwheelChange, PitchbendChange, Settings, KeyHoldToggle
}

public class ActionData
{
    public Action<float> actionEvent;
    public float floatInput;

    public ActionData(Action<float> newAction, float initialValue = 0f)
    {
        actionEvent = newAction ?? (_ => { });
        floatInput = initialValue;
    }
}

public class InputManager : Singleton<InputManager>
{
    public bool usingMidiKeyboard { get; private set; }

    [SerializeField] private InputActionAsset keyInputs;
    private bool checkingKeys, keyToggle;
    private float verticalArrowSpeed, heldNote;
    private Key lastKey = Key.None, lastNum = Key.None;

    private readonly List<int> notesDown = new();
    private readonly List<int> notesUp = new();
    private readonly List<IInputScript> inputs = new();
    private readonly Dictionary<ActionTypes, ActionData> actionDict = new();

    private float chordTimeWindow = 0.05f;

    private MidiDevice midiController;
    private IInputDevice midiPitchController;

    private readonly Dictionary<Key, int> numOctMapping = new()
    {
        { Key.Digit1, 0 }, { Key.Digit2, 1 }, { Key.Digit3, 2 }, { Key.Digit4, 3 }, { Key.Digit5, 4 }
    };

    private readonly Dictionary<Key, int> keyNoteMapping = new()
    {
        { Key.A, 1 }, { Key.S, 2 }, { Key.D, 3 }, { Key.F, 4 }, { Key.G, 5 }
    };

    void Start()
    {
        if (!DH.Get<TestOverrides>().ignoreMidi)
        {
            usingMidiKeyboard = false;
            InputSystem.onDeviceChange += OnMidiDeviceChange;
        }
        else
        {
            UseKeyboard();
        }
    }

    void OnMidiDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change != InputDeviceChange.Added) return;

        midiController = device as MidiDevice;
        midiPitchController = Melanchall.DryWetMidi.Multimedia.InputDevice.GetByIndex(0);
        midiPitchController.StartEventsListening();

        usingMidiKeyboard = midiController != null;

        if (usingMidiKeyboard)
        {
            InitializeGlobalActions();
            InitializeMidiActions();
            ConfigureInputs();
        }
    }

    void UseKeyboard()
    {
        usingMidiKeyboard = false;
        InitializeGlobalActions();
        InitializeCPUActions();
        ConfigureInputs();
    }

    void InitializeGlobalActions()
    {
        if (actionDict.Count == 0)
        {
            foreach (ActionTypes type in Enum.GetValues(typeof(ActionTypes)))
                actionDict[type] = new ActionData(_ => { });

            actionDict[ActionTypes.Settings].floatInput = DH.Get<TestOverrides>().ignoreMidi ? 0 : 1;
        }

        keyInputs.FindActionMap("KeyboardMapping").FindAction("Settings").performed += _ =>
        {
            actionDict[ActionTypes.Settings].floatInput = actionDict[ActionTypes.Settings].floatInput < 1 ? 1 : 0;
            actionDict[ActionTypes.Settings].actionEvent.Invoke(actionDict[ActionTypes.Settings].floatInput);
        };
    }

    void InitializeMidiActions()
    {
        midiController.onWillNoteOn += (note, velocity) =>
        {
            if (GetNote(note.noteNumber) is > 0 and < 26)
            {
                notesDown.Add((int)GetNote(note.noteNumber));
                CRManager.root.Begin(DelayedNoteCheck(ActionTypes.KeyDown), "NoteDownCheck", this);
            }

        };
        midiController.onWillNoteOff += note =>
        {
            if (GetNote(note.noteNumber) is > 0 and < 26)
            {
                notesUp.Add((int)GetNote(note.noteNumber));
                CRManager.root.Begin(DelayedNoteCheck(ActionTypes.KeyUp), "NoteUpCheck", this);
            }
        };

        midiController.onWillControlChange += (wheel, amount) =>
        {
            if (wheel.controlNumber == 1)
            {
                actionDict[ActionTypes.ModwheelChange].actionEvent.Invoke(amount);
            }
            else
            {
                var root = ConfigureKeyboard.root;
                if (root.settingsCC == wheel.controlNumber)
                {
                    actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                }
                else if (root.tempSettingsCC == wheel.controlNumber)
                {
                    root.settingsCC = wheel.controlNumber;
                    actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                }
                else if (root.tempSettingsCC == -1 || root.tempSettingsCC != wheel.controlNumber)
                {
                    root.tempSettingsCC = wheel.controlNumber;
                    actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                }
            }
        };

        midiPitchController.EventReceived += (_, message) =>
        {
            if (message.Event is PitchBendEvent p)
                actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke((p.PitchValue - 8192f) / 8192f);
        };
    }
    IEnumerator DelayedNoteCheck(ActionTypes type)
    {
        yield return new WaitForSeconds(chordTimeWindow);

        if (type == ActionTypes.KeyDown)
        {
            if (notesDown.Count == 4)
            {
                int encodedChord = 0;
                notesDown.Sort();

                foreach (int note in notesDown)
                {
                    encodedChord |= 1 << note;
                }

                bool invokedChord = false;

                for (int i = 0; i < BrChord.root.abilities.Length; i++)
                {
                    int compareChord = 0;
                    foreach (var note in BrChord.root.abilities[i].notes)
                    {
                        compareChord |= 1 << (int)PuzzleUtilities.root.GetNoteNumber(note);
                    }

                    if (encodedChord == compareChord)
                    {
                        actionDict[ActionTypes.ChordDown].actionEvent.Invoke(i);
                        invokedChord = true;
                        i = BrChord.root.abilities.Length;
                    }
                }

                if (!invokedChord)
                {
                    foreach (float note in notesDown)
                    {
                        actionDict[ActionTypes.KeyDown].actionEvent.Invoke(note);
                    }
                }
            }
            else
            {
                foreach (float note in notesDown)
                {
                    actionDict[ActionTypes.KeyDown].actionEvent.Invoke(note);
                }
            }

            notesDown.Clear();
        }
        else if (type == ActionTypes.KeyUp)
        {
            if (notesUp.Count == 4)
            {
                int encodedChord = 0;
                notesUp.Sort();

                foreach (int note in notesUp)
                {
                    encodedChord |= 1 << note;
                }

                bool invokedChord = false;

                for (int i = 0; i < BrChord.root.abilities.Length; i++)
                {
                    int compareChord = 0;
                    foreach (var note in BrChord.root.abilities[i].notes)
                    {
                        compareChord |= 1 << (int)PuzzleUtilities.root.GetNoteNumber(note);
                    }

                    if (encodedChord == compareChord)
                    {
                        actionDict[ActionTypes.ChordUp].actionEvent.Invoke(i);
                        invokedChord = true;
                        i = BrChord.root.abilities.Length;
                    }
                }

                if (!invokedChord)
                {
                    foreach (float note in notesUp)
                    {
                        actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note);
                    }
                }
            }
            else
            {
                foreach (float note in notesUp)
                {
                    actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note);
                }
            }

            notesUp.Clear();
        }
    }
    float GetNote(float noteCheck)
    {
        return ConfigureKeyboard.root.middleKey != -1
            ? noteCheck - ConfigureKeyboard.root.middleKey + 13
            : noteCheck;
    }

    void InitializeCPUActions()
    {
        var map = keyInputs.FindActionMap("KeyboardMapping");

        map.FindAction("KeyDown").performed += _ => checkingKeys = true;
        map.FindAction("KeyUp").performed += _ =>
        {
            checkingKeys = false;
            if (heldNote > 0)
            {
                actionDict[ActionTypes.KeyUp].actionEvent.Invoke(heldNote);
                heldNote = 0;
            }
            lastKey = Key.None;
            lastNum = Key.None;
        };

        map.FindAction("Toggle").performed += _ =>
        {
            keyToggle = !keyToggle;
            if (!keyToggle)
            {
                foreach (float note in notesDown)
                    actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note);
                notesDown.Clear();
            }

            lastNum = Key.None;
            lastKey = Key.None;
        };

        map.FindAction("ArrowVertical").started += amount => verticalArrowSpeed = amount.ReadValue<float>() * 0.01f;
        map.FindAction("ArrowVertical").canceled += _ => verticalArrowSpeed = 0;

        map.FindAction("ArrowHorizontal").performed += amount => actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke(amount.ReadValue<float>());
        map.FindAction("ArrowHorizontal").canceled += _ => actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke(0);
    }

    void Update()
    {
        if (checkingKeys) ProcessKeyInput();

        if (verticalArrowSpeed != 0)
        {
            var modWheel = actionDict[ActionTypes.ModwheelChange];
            modWheel.floatInput = Mathf.Clamp(modWheel.floatInput + verticalArrowSpeed, 0, 1);
            modWheel.actionEvent.Invoke(modWheel.floatInput);
        }
    }

    void ProcessKeyInput()
    {
        if (keyToggle)
        {
            foreach (var num in numOctMapping.Keys)
                if (Keyboard.current[num].wasPressedThisFrame) lastNum = num;

            foreach (var key in keyNoteMapping.Keys)
                if (Keyboard.current[key].wasPressedThisFrame) lastKey = key;

            if (lastNum != Key.None && lastKey != Key.None)
            {
                int note = numOctMapping[lastNum] * 5 + keyNoteMapping[lastKey];

                if (notesDown.Contains(note))
                {
                    notesDown.Remove(note);
                    actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note);
                }
                else
                {
                    notesDown.Add(note);
                    actionDict[ActionTypes.KeyDown].actionEvent.Invoke(note);
                }

                lastNum = Key.None;
                lastKey = Key.None;
            }
        }
        else
        {
            foreach (var num in numOctMapping.Keys)
            {
                if (Keyboard.current[num].wasPressedThisFrame) lastNum = num;

                else if (Keyboard.current[num].wasPressedThisFrame && lastNum == num) lastNum = Key.None;
            }
            foreach (var key in keyNoteMapping.Keys)
            {
                if (Keyboard.current[key].wasPressedThisFrame) lastKey = key;

                else if (Keyboard.current[key].wasPressedThisFrame && lastNum == key) lastKey = Key.None;
            }

            if (lastNum != Key.None && lastKey != Key.None)
            {
                if (heldNote != numOctMapping[lastNum] * 5 + keyNoteMapping[lastKey])
                {
                    if (heldNote > 0)
                        actionDict[ActionTypes.KeyUp].actionEvent.Invoke(heldNote);

                    actionDict[ActionTypes.KeyDown].actionEvent.Invoke(numOctMapping[lastNum] * 5 + keyNoteMapping[lastKey]);
                    heldNote = numOctMapping[lastNum] * 5 + keyNoteMapping[lastKey];
                }
            }
            else if (heldNote > 0)
            {
                actionDict[ActionTypes.KeyUp].actionEvent.Invoke(heldNote);
                heldNote = 0;
            }
        }
    }
    void ConfigureInputs()
    {
        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var behaviour in allBehaviours)
        {
            var inputScripts = behaviour.GetComponents<IInputScript>();

            if (inputScripts != null)
            {
                foreach (var input in inputScripts)
                {
                    if (!inputs.Contains(input))
                    {
                        inputs.Add(input);
                        input.AddInputs();
                    }
                }

            }
        }
    }
    public void AddListener<T>(ActionTypes type, Action<float> method)
    {
        var stateAttribute = (AllowedStatesAttribute)Attribute.GetCustomAttribute(method.Method, typeof(AllowedStatesAttribute));

        Action<float> wrappedMethod = method;
        if (stateAttribute != null)
        {
            wrappedMethod = input =>
            {
                if (Array.Exists(stateAttribute.States, state => state == GameManager.root.currentState))
                {
                    method(input);
                }
            };
        }

        lock (actionDict)
        {
            actionDict[type].actionEvent += wrappedMethod;
        }
    }

    void OnDisable()
    {
        if(midiPitchController != null)
            midiPitchController.StopEventsListening();
    }
}
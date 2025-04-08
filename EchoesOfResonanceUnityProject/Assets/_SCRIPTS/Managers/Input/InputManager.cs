using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System.Collections;
using System.Linq;

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
    [SerializeField] private InputActionAsset keyInputs;
    [SerializeField] private GameObject _configScreen;
    private bool checkingKeys, keyToggle;
    private float verticalArrowSpeed, heldNote;
    private Key lastKey = Key.None, lastNum = Key.None;
    private Action<char> _subscribeCPU;
    private readonly List<int> notesDown = new();
    private readonly List<int> notesUp = new();
    private readonly List<IInputScript> inputs = new();
    private readonly Dictionary<ActionTypes, ActionData> actionDict = new();
    private IInputDevice _midiController;

    private readonly Dictionary<Key, int> numOctMapping = new()
    {
        { Key.Digit1, 0 }, { Key.Digit2, 1 }, { Key.Digit3, 2 }, { Key.Digit4, 3 }, { Key.Digit5, 4 }
    };

    private readonly Dictionary<Key, int> keyNoteMapping = new()
    {
        { Key.A, 1 }, { Key.S, 2 }, { Key.D, 3 }, { Key.F, 4 }, { Key.G, 5 }
    };
    protected override void Awake()
    {
        base.Awake();
        InitializeGlobalActions();

        UIFade.root.SetAlpha(1, new() { _configScreen });

        _midiController = Melanchall.DryWetMidi.Multimedia.InputDevice.GetAll().FirstOrDefault();
        _midiController.EventReceived += UseMIDI;
        _midiController.StartEventsListening();

        _subscribeCPU += _ =>
        {
            UseKeyboard();
            CRManager.root.Begin(UIFade.root.FadeItems(0.5f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { _configScreen }), "FadeOutConfig", this);
            MusicManager.root.MusicToGameplay();
        };

        Keyboard.current.onTextInput += _subscribeCPU;
    }

    void InitializeGlobalActions()
    {
        if (actionDict.Count == 0)
        {
            foreach (ActionTypes type in Enum.GetValues(typeof(ActionTypes)))
                actionDict[type] = new ActionData(_ => { });
        }

        keyInputs.FindActionMap("KeyboardMapping").FindAction("Settings").performed += _ =>
        {
            actionDict[ActionTypes.Settings].floatInput = actionDict[ActionTypes.Settings].floatInput < 1 ? 1 : 0;
            actionDict[ActionTypes.Settings].actionEvent.Invoke(actionDict[ActionTypes.Settings].floatInput);
        };
    }

    void UseMIDI(object sender, MidiEventReceivedEventArgs e)
    {
        if (!(e.Event is NoteOnEvent or NoteOffEvent or ControlChangeEvent or PitchBendEvent)) return;

        UnityMainThread.wkr.AddJob(() =>
        {
            _midiController.EventReceived -= UseMIDI;
            Keyboard.current.onTextInput -= _subscribeCPU;

            keyInputs.FindActionMap("KeyboardMapping").Disable();
            keyInputs.FindActionMap("KeyboardMapping").FindAction("Settings").Enable();
            Keyboard.current.onTextInput -= _ => UseKeyboard();

            CRManager.root.Begin(UIFade.root.FadeItems(0.5f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { _configScreen }), "FadeOutConfig", this);
            MusicManager.root.MusicToGameplay();

            InitializeMidiActions();
            ConfigureInputs();
        });
    }

    void UseKeyboard()
    {
        _midiController.EventReceived -= UseMIDI;
        Keyboard.current.onTextInput -= _subscribeCPU;

        keyInputs.FindActionMap("KeyboardMapping").Enable();

        InitializeCPUActions();
        ConfigureInputs();
    }

    void InitializeMidiActions()
    {
        _midiController.EventReceived += (_, message) =>
        {
            UnityMainThread.wkr.AddJob(() =>
            {
                switch (message.Event)
                {
                    case NoteOnEvent n:

                        notesDown.Add((int)GetNote((byte)n.NoteNumber));
                        CRManager.root.Begin(DelayedNoteCheck(ActionTypes.KeyDown), "NoteDownCheck", this);

                        break;

                    case NoteOffEvent f:

                        notesUp.Add((int)GetNote((byte)f.NoteNumber));
                        CRManager.root.Begin(DelayedNoteCheck(ActionTypes.KeyUp), "NoteUpCheck", this);

                        break;

                    case ControlChangeEvent c:

                        float controlInput = (byte)c.ControlValue / 127f;

                        if (c.ControlNumber == 1)
                        {
                            actionDict[ActionTypes.ModwheelChange].actionEvent.Invoke(controlInput);
                        }
                        else
                        {
                            actionDict[ActionTypes.Settings].actionEvent.Invoke(controlInput);
                        }

                        break;

                    case PitchBendEvent p:

                        actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke((p.PitchValue - 8192f) / 8192f);

                        break;
                }
            });
        };
    }

    IEnumerator DelayedNoteCheck(ActionTypes type)
    {
        yield return new WaitForSeconds(0.05f);

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
        return BrSettings.root.middleKey != -1
            ? Mathf.Clamp(noteCheck - BrSettings.root.middleKey + 13, 1, 25)
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

        map.FindAction("ArrowVertical").started += amount => verticalArrowSpeed = amount.ReadValue<float>() * 0.0325f;
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
        if (_midiController != null)
            _midiController.StopEventsListening();

        Keyboard.current.onTextInput -= _ =>
        {
            UseKeyboard();
            CRManager.root.Begin(UIFade.root.FadeItems(0.5f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { _configScreen }), "FadeOutConfig", this);
        };
    }
}
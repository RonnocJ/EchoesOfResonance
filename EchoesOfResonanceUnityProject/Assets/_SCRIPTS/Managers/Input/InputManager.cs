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
    KeyDown, KeyUp, ChordDown, ModwheelChange, PitchbendChange, Settings, KeyHoldToggle
}

public class ActionData
{
    public Action<float> actionEvent;
    public float floatInput;
    public Action<int, int> actionWithVelocity;

    public ActionData(Action<float> newAction, float initialValue = 0f, Action<int, int> newVelocityAction = null)
    {
        actionEvent = newAction ?? (_ => { });
        floatInput = initialValue;
        actionWithVelocity = newVelocityAction ?? null;
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
    private readonly Dictionary<int, int> notesDown = new();
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

        UIUtil.root.SetAlpha(1, new() { _configScreen });

        _midiController = Melanchall.DryWetMidi.Multimedia.InputDevice.GetAll().FirstOrDefault();

        if (_midiController != null)
        {
            _midiController.EventReceived += UseMIDI;
            _midiController.StartEventsListening();
        }

        _subscribeCPU += _ =>
        {
            UseKeyboard();
            CRManager.root.Begin(UIUtil.root.FadeItems(0.5f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { _configScreen }), "FadeOutConfig", this);
            MusicManager.root.MusicToGameplay();
        };

        Keyboard.current.onTextInput += _subscribeCPU;
    }

    void InitializeGlobalActions()
    {
        if (actionDict.Count == 0)
        {
            foreach (ActionTypes type in Enum.GetValues(typeof(ActionTypes)))
            {
                if (type is ActionTypes.KeyDown or ActionTypes.ChordDown)
                {
                    actionDict[type] = new ActionData(_ => { }, 0, (_, _) => { });
                }
                else
                {
                    actionDict[type] = new ActionData(_ => { });
                }
            }

        }

        keyInputs.FindActionMap("KeyboardMapping").FindAction("Settings").performed += _ =>
        {
            actionDict[ActionTypes.Settings].floatInput = actionDict[ActionTypes.Settings].floatInput < 1 ? 1 : 0;
            actionDict[ActionTypes.Settings].actionEvent.Invoke(actionDict[ActionTypes.Settings].floatInput);
        };
    }

    void UseMIDI(object sender, MidiEventReceivedEventArgs e)
    {
        if (!(e.Event is NoteOnEvent)) return;

        UnityMainThread.wkr.AddJob(() =>
        {
            _midiController.EventReceived -= UseMIDI;
            Keyboard.current.onTextInput -= _subscribeCPU;

            keyInputs.FindActionMap("KeyboardMapping").Disable();
            keyInputs.FindActionMap("KeyboardMapping").FindAction("Settings").Enable();
            Keyboard.current.onTextInput -= _ => UseKeyboard();

            CRManager.root.Begin(UIUtil.root.FadeItems(0.5f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { _configScreen }), "FadeOutConfig", this);
            MusicManager.root.MusicToGameplay();

            InitializeMidiActions();
            ConfigureInputs();
        });
    }

    void UseKeyboard()
    {
        if (_midiController != null) _midiController.EventReceived -= UseMIDI;
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
                        if ((int)GetNote((byte)n.NoteNumber) is >= 1 and <= 25)
                        {
                            notesDown[(int)GetNote((byte)n.NoteNumber)] = (int)n.Velocity;
                            CRManager.root.Begin(DelayedNoteCheck(ActionTypes.KeyDown), "NoteDownCheck", this);
                        }
                        break;

                    case NoteOffEvent f:
                        if ((int)GetNote((byte)f.NoteNumber) is >= 1 and <= 25)
                        {
                            notesUp.Add((int)GetNote((byte)f.NoteNumber));
                            CRManager.root.Begin(DelayedNoteCheck(ActionTypes.KeyUp), "NoteUpCheck", this);
                        }
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
                int averageVelocity = 0;

                foreach (var n in notesDown)
                {
                    encodedChord |= 1 << n.Key;
                    averageVelocity += n.Value;
                }

                bool invokedChord = false;

                for (int i = 0; i < BrChord.root.abilities.Length; i++)
                {
                    int compareChord = 0;
                    foreach (var note in BrChord.root.abilities[i].notes)
                    {
                        compareChord |= 1 << (int)PzUtil.GetNoteNumber(note);
                    }

                    if (encodedChord == compareChord)
                    {
                        actionDict[ActionTypes.ChordDown].actionEvent.Invoke(i);
                        actionDict[ActionTypes.ChordDown].actionWithVelocity.Invoke(i, averageVelocity / 4);
                        invokedChord = true;
                        i = BrChord.root.abilities.Length;
                    }
                }

                if (!invokedChord)
                {
                    foreach (var n in notesDown)
                    {
                        actionDict[ActionTypes.KeyDown].actionEvent.Invoke(n.Key);
                        actionDict[ActionTypes.KeyDown].actionWithVelocity.Invoke(n.Key, n.Value);
                    }
                }
            }
            else
            {
                foreach (var n in notesDown)
                {
                    actionDict[ActionTypes.KeyDown].actionEvent.Invoke(n.Key);
                    actionDict[ActionTypes.KeyDown].actionWithVelocity.Invoke(n.Key, n.Value);
                }
            }

            notesDown.Clear();
        }
        else if (type == ActionTypes.KeyUp)
        {
            foreach (float note in notesUp)
            {
                actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note);
            }

            notesUp.Clear();
        }
    }
    float GetNote(float noteCheck)
    {
        return BrDisplay.root.middleKey != -1
            ? noteCheck - BrDisplay.root.middleKey + 13
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
                foreach (var n in notesDown)
                    actionDict[ActionTypes.KeyUp].actionEvent.Invoke(n.Key);
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

                if (notesDown.ContainsKey(note))
                {
                    notesDown.Remove(note);
                    actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note);
                }
                else
                {
                    notesDown.Add(note, 64);
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
        var allowedStateAttribute = (AllowedStates)Attribute.GetCustomAttribute(method.Method, typeof(AllowedStates));
        var dissallowedStateAttribute = (DissallowedStates)Attribute.GetCustomAttribute(method.Method, typeof(DissallowedStates));
        var allStateAttribute = (AllowAllAboveState)Attribute.GetCustomAttribute(method.Method, typeof(AllowAllAboveState));

        Action<float> wrappedMethod = input =>
        {
            var State = GameManager.root.State;

            if (allowedStateAttribute != null && !allowedStateAttribute.States.Contains(State))
                return;

            if (dissallowedStateAttribute != null && dissallowedStateAttribute.States.Contains(State))
                return;

            if (allStateAttribute != null && State < allStateAttribute.MinState)
                return;


            method(input);
        };

        lock (actionDict)
        {
            actionDict[type].actionEvent += wrappedMethod;
        }
    }

    public void AddVelocityListener<T>(ActionTypes type, Action<int, int> method)
    {
        var allowedStateAttribute = (AllowedStates)Attribute.GetCustomAttribute(method.Method, typeof(AllowedStates));
        var dissallowedStateAttribute = (DissallowedStates)Attribute.GetCustomAttribute(method.Method, typeof(DissallowedStates));
        var allStateAttribute = (AllowAllAboveState)Attribute.GetCustomAttribute(method.Method, typeof(AllowAllAboveState));

        Action<int, int> wrappedMethod = (note, vel) =>
        {
            var State = GameManager.root.State;

            if (allowedStateAttribute != null && !allowedStateAttribute.States.Contains(State))
                return;

            if (dissallowedStateAttribute != null && dissallowedStateAttribute.States.Contains(State))
                return;

            if (allStateAttribute != null && State >= allStateAttribute.MinState)
                return;


            method(note, vel);
        };

        lock (actionDict)
        {
            actionDict[type].actionWithVelocity += wrappedMethod;
        }
    }
    public void AllNotesOff()
    {
        for (int i = 1; i < 26; i++)
        {
            actionDict[ActionTypes.KeyUp].actionEvent.Invoke(i);
        }
    }
    void OnDisable()
    {
        if (_midiController != null)
        {
            _midiController.StopEventsListening();
            _midiController.EventReceived -= UseMIDI;
        }

        Keyboard.current.onTextInput -= _subscribeCPU;
    }
}
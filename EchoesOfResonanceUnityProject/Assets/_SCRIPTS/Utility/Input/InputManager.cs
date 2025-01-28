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
    PitchbendChange,
    Settings
}
[Serializable]
public class ActionData
{
    [HideInInspector]
    public Action<float> actionEvent;
    [HideInInspector]
    public float floatInput;
    public ActionData(Action<float> newAction)
    {
        actionEvent = newAction;
    }
}
public class InputManager : Singleton<InputManager>
{
    [HideInInspector]
    public bool usingMidiKeyboard, usingEscKey, settingsToggleState;
    [SerializeField] private InputActionAsset keyInputs;
    private bool checkingKeys, verticalArrowHeld;
    public float arrowInput, arrowDirection;
    private HashSet<Key> heldKeys = new();
    private List<IInputScript> inputs = new List<IInputScript>();
    private Dictionary<ActionTypes, ActionData> actionDict = new Dictionary<ActionTypes, ActionData>();
    private MidiDevice midiController;
    private Melanchall.DryWetMidi.Multimedia.IInputDevice midiPitchController;
    private Dictionary<Key, float> keyNoteMapping = new Dictionary<Key, float>
{
    { Key.Q, 1f },
    { Key.W, 2f },
    { Key.E, 3f },
    { Key.R, 4f },
    { Key.T, 5f },
    { Key.Y, 6f },
    { Key.U, 7f },
    { Key.I, 8f },
    { Key.O, 9f },
    { Key.A, 10f },
    { Key.S, 11f },
    { Key.D, 12f },
    { Key.F, 13f },
    { Key.G, 14f },
    { Key.H, 15f },
    { Key.J, 16f },
    { Key.K, 17f },
    { Key.L, 18f },
    { Key.Z, 19f },
    { Key.X, 20f },
    { Key.C, 21f },
    { Key.V, 22f },
    { Key.B, 23f },
    { Key.N, 24f },
    { Key.M, 25f },
};

    void Start()
    {
        if (!TestOverrides.root.ignoreMidi)
        {
            usingMidiKeyboard = false;
            InputSystem.onDeviceChange += (device, change) =>
            {
                if (change != InputDeviceChange.Added) return;
    
                midiController = device as MidiDevice;
                midiPitchController = Melanchall.DryWetMidi.Multimedia.InputDevice.GetByIndex(0);
                midiPitchController.StartEventsListening();
    
                if (midiController == null) return;
                usingMidiKeyboard = true;
                InitializeActions();
                ConfigureInputs();
            };
        }
        else
        {
            UseKeyboard();
        }
    }
    public void UseKeyboard()
    {
        settingsToggleState = false;
        usingMidiKeyboard = false;

        InitializeActions();
        ConfigureInputs();
    }
    void InitializeActions()
    {
        if (actionDict.Count == 0)
        {
            actionDict.Add(ActionTypes.KeyDown, new ActionData(new Action<float>((_) => { })));
            actionDict.Add(ActionTypes.KeyUp, new ActionData(new Action<float>((_) => { })));
            actionDict.Add(ActionTypes.ModwheelChange, new ActionData(new Action<float>((_) => { })));
            actionDict.Add(ActionTypes.PitchbendChange, new ActionData(new Action<float>((_) => { })));
            actionDict.Add(ActionTypes.Settings, new ActionData(new Action<float>((_) => { })));
        }

        if (usingMidiKeyboard)
        {
            midiController.onWillNoteOn += (note, velocity) =>
            {

                if (ConfigureKeyboard.root.middleKey != -1)
                {
                    float trueNoteNumber = note.noteNumber - ConfigureKeyboard.root.middleKey + 13;

                    if (trueNoteNumber > 0 && trueNoteNumber < 26)
                    {
                        actionDict[ActionTypes.KeyDown].actionEvent.Invoke(trueNoteNumber);
                    }
                }
                else
                {
                    actionDict[ActionTypes.KeyDown].actionEvent.Invoke(note.noteNumber);
                }
            };

            midiController.onWillNoteOff += note =>
            {
                if (ConfigureKeyboard.root.middleKey != -1)
                {
                    float trueNoteNumber = note.noteNumber - ConfigureKeyboard.root.middleKey + 13;

                    if (trueNoteNumber > 0 && trueNoteNumber < 26)
                    {
                        actionDict[ActionTypes.KeyUp].actionEvent.Invoke(trueNoteNumber);
                    }
                }
                else
                {
                    actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note.noteNumber);
                }

            };

            midiController.onWillControlChange += (wheel, amount) =>
            {
                if (wheel.controlNumber == 1)
                {
                    actionDict[ActionTypes.ModwheelChange].actionEvent.Invoke(amount);
                }
            };

            midiPitchController.EventReceived += (sender, message) =>
            {
                if (message.Event is Melanchall.DryWetMidi.Core.PitchBendEvent p)
                {
                    actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke((p.PitchValue - 8192f) / 8192f);
                }
            };

            midiController.onWillControlChange += (knob, amount) =>
            {
                if (knob.controlNumber != 1)
                {
                    if (ConfigureKeyboard.root.tempSettingsCC == -1)
                    {
                        actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                        ConfigureKeyboard.root.tempSettingsCC = knob.controlNumber;

                    }
                    else if (ConfigureKeyboard.root.tempSettingsCC != knob.controlNumber)
                    {
                        ConfigureKeyboard.root.tempSettingsCC = knob.controlNumber;
                        actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                    }
                    else if (ConfigureKeyboard.root.tempSettingsCC == knob.controlNumber && amount == 0)
                    {
                        ConfigureKeyboard.root.settingsCC = knob.controlNumber;
                        actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                    }
                    else if (ConfigureKeyboard.root.settingsCC == knob.controlNumber)
                    {
                        actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                    }
                }
            };
        }
        else
        {
            InputActionMap map = keyInputs.FindActionMap("KeyboardMapping");

            map.FindAction("KeyDown").performed += _ => checkingKeys = true;
            map.FindAction("KeyUp").performed += _ =>
            {
                checkingKeys = false;
                foreach (var key in keyNoteMapping.Keys)
                {
                    var keyControl = Keyboard.current[key];
                    if (keyControl.wasReleasedThisFrame && heldKeys.Remove(key))
                        actionDict[ActionTypes.KeyUp].actionEvent.Invoke(keyNoteMapping[key]);
                }
            };

            map.FindAction("ArrowVertical").performed += amount =>
            {
                verticalArrowHeld = true;
                arrowDirection = amount.ReadValue<float>();
            };
            map.FindAction("ArrowVertical").canceled += _ => verticalArrowHeld = false;

            map.FindAction("ArrowHorizontal").performed += amount => actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke(amount.ReadValue<float>());
            map.FindAction("ArrowHorizontal").canceled += _ => actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke(0);
        }

        keyInputs.FindActionMap("KeyboardMapping").FindAction("Settings").performed += _ =>
        {
            usingEscKey = true;
            settingsToggleState = !settingsToggleState;
            actionDict[ActionTypes.Settings].actionEvent.Invoke(settingsToggleState ? 1f : 0f);
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
    void Update()
    {
        if (checkingKeys)
        {
            foreach (var key in keyNoteMapping.Keys)
            {
                var keyControl = Keyboard.current[key];
                if (
                    keyControl.wasPressedThisFrame &&
                    Enum.TryParse(keyControl.keyCode.ToString(), out Key heldKey) &&
                    heldKeys.Add(heldKey)
                )
                    actionDict[ActionTypes.KeyDown].actionEvent.Invoke(keyNoteMapping[key]);

                else if (keyControl.wasReleasedThisFrame && heldKeys.Remove(key))
                    actionDict[ActionTypes.KeyUp].actionEvent.Invoke(keyNoteMapping[key]);
            }
        }

        if (verticalArrowHeld)
        {
            arrowInput += arrowDirection * Time.deltaTime;
            arrowInput = Mathf.Clamp(arrowInput, 0, 1);
            actionDict[ActionTypes.ModwheelChange].actionEvent.Invoke(arrowInput);
        }
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= (device, change) =>
        {
            if (change != InputDeviceChange.Added) return;

            midiController = device as MidiDevice;
            midiPitchController = Melanchall.DryWetMidi.Multimedia.InputDevice.GetByIndex(0);
            midiPitchController.StartEventsListening();

            if (midiController == null) return;
            usingMidiKeyboard = true;
            InitializeActions();
            ConfigureInputs();
        };

        if (GameManager.root.currentState != GameState.Config)
        {
            if (usingMidiKeyboard)
            {
                midiController.onWillNoteOn -= (note, velocity) =>
                {

                    if (ConfigureKeyboard.root.middleKey != -1)
                    {
                        float trueNoteNumber = note.noteNumber - ConfigureKeyboard.root.middleKey + 13;

                        if (trueNoteNumber > 0 && trueNoteNumber < 26)
                        {
                            actionDict[ActionTypes.KeyDown].actionEvent.Invoke(trueNoteNumber);
                        }
                    }
                    else
                    {
                        actionDict[ActionTypes.KeyDown].actionEvent.Invoke(note.noteNumber);
                    }
                };

                midiController.onWillNoteOff -= note =>
                {
                    if (ConfigureKeyboard.root.middleKey != -1)
                    {
                        float trueNoteNumber = note.noteNumber - ConfigureKeyboard.root.middleKey + 13;

                        if (trueNoteNumber > 0 && trueNoteNumber < 26)
                        {
                            actionDict[ActionTypes.KeyUp].actionEvent.Invoke(trueNoteNumber);
                        }
                    }
                    else
                    {
                        actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note.noteNumber);
                    }

                };

                midiController.onWillControlChange -= (wheel, amount) =>
                {
                    if (wheel.controlNumber == 1)
                    {
                        actionDict[ActionTypes.ModwheelChange].actionEvent.Invoke(amount);
                    }
                };

                midiPitchController.EventReceived -= (sender, message) =>
                {
                    if (message.Event is Melanchall.DryWetMidi.Core.PitchBendEvent p)
                    {
                        actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke((p.PitchValue - 8192f) / 8192f);
                    }
                };

                midiController.onWillControlChange -= (knob, amount) =>
                {
                    if (knob.controlNumber != 1)
                    {
                        if (ConfigureKeyboard.root.tempSettingsCC == -1)
                        {
                            actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                            ConfigureKeyboard.root.tempSettingsCC = knob.controlNumber;

                        }
                        else if (ConfigureKeyboard.root.tempSettingsCC != knob.controlNumber)
                        {
                            ConfigureKeyboard.root.tempSettingsCC = knob.controlNumber;
                            actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                        }
                        else if (ConfigureKeyboard.root.tempSettingsCC == knob.controlNumber && amount == 0)
                        {
                            ConfigureKeyboard.root.settingsCC = knob.controlNumber;
                            actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                        }
                        else if (ConfigureKeyboard.root.settingsCC == knob.controlNumber)
                        {
                            actionDict[ActionTypes.Settings].actionEvent.Invoke(amount);
                        }
                    }
                };
            }
            else
            {
                InputActionMap map = keyInputs.FindActionMap("KeyboardMapping");

                map.FindAction("KeyDown").performed -= _ => checkingKeys = true;
                map.FindAction("KeyUp").performed -= _ =>
                {
                    checkingKeys = false;
                    foreach (var key in keyNoteMapping.Keys)
                    {
                        var keyControl = Keyboard.current[key];
                        if (keyControl.wasReleasedThisFrame && heldKeys.Remove(key))
                            actionDict[ActionTypes.KeyUp].actionEvent.Invoke(keyNoteMapping[key]);
                    }
                };

                map.FindAction("ArrowVertical").performed -= amount =>
                {
                    verticalArrowHeld = true;
                    arrowDirection = amount.ReadValue<float>();
                };
                map.FindAction("ArrowVertical").canceled -= _ => verticalArrowHeld = false;

                map.FindAction("ArrowHorizontal").performed -= amount => actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke(amount.ReadValue<float>());
                map.FindAction("ArrowHorizontal").canceled -= _ => actionDict[ActionTypes.PitchbendChange].actionEvent.Invoke(0);
            }

            keyInputs.FindActionMap("KeyboardMapping").FindAction("Settings").performed -= _ =>
            {
                Debug.Log("pressed escape key");
                usingEscKey = true;
                settingsToggleState = !settingsToggleState;
                actionDict[ActionTypes.Settings].actionEvent.Invoke(settingsToggleState ? 1f : 0f);
            };
        }
    }
}
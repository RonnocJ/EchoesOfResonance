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
    Settings,
    KeyHoldToggle
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
    private bool checkingKeys, keyToggle, verticalArrowHeld;
    public float arrowInput, arrowDirection;
    private HashSet<float> heldNotes = new();
    private float heldNote;
    private Key lastKey, lastNum;
    private List<IInputScript> inputs = new List<IInputScript>();
    private Dictionary<ActionTypes, ActionData> actionDict = new Dictionary<ActionTypes, ActionData>();
    private MidiDevice midiController;
    private Melanchall.DryWetMidi.Multimedia.IInputDevice midiPitchController;
    private Dictionary<Key, float> numberOctaveMapping = new Dictionary<Key, float>
{
    { Key.Digit1, 0f },
    { Key.Digit2, 1f },
    { Key.Digit3, 2f },
    { Key.Digit4, 3f },
    { Key.Digit5, 4f },
};
    private Dictionary<Key, float> keyNoteMapping = new Dictionary<Key, float>
{
    { Key.A, 1f },
    { Key.S, 2f },
    { Key.D, 3f },
    { Key.F, 4f },
    { Key.G, 5f },
};

    void Start()
    {
        if (!DH.Get<TestOverrides>().ignoreMidi)
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

                if (!keyToggle)
                {
                    if (heldNote > 0)
                    {
                        actionDict[ActionTypes.KeyUp].actionEvent.Invoke(heldNote);
                        heldNote = 0;
                    }

                    lastKey = Key.None;
                    lastNum = Key.None;
                }

            };

            map.FindAction("Toggle").performed += _ =>
            {
                keyToggle = !keyToggle;

                if (!keyToggle)
                {
                    foreach (float note in heldNotes)
                    {
                        actionDict[ActionTypes.KeyUp].actionEvent.Invoke(note);
                    }
                    heldNotes.Clear();
                }

                lastNum = Key.None;
                lastKey = Key.None;
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
            if (keyToggle)
            {
                foreach (var num in numberOctaveMapping.Keys)
                {
                    var keyControl = Keyboard.current[num];
                    if (keyControl.wasPressedThisFrame && Enum.TryParse(keyControl.keyCode.ToString(), out Key heldNum))
                        lastNum = heldNum;
                }
                foreach (var key in keyNoteMapping.Keys)
                {
                    var keyControl = Keyboard.current[key];
                    if (keyControl.wasPressedThisFrame && Enum.TryParse(keyControl.keyCode.ToString(), out Key heldKey))
                        lastKey = heldKey;
                }

                if (lastNum != Key.None && lastKey != Key.None)
                {
                    if (heldNotes.Remove(numberOctaveMapping[lastNum] * 5 + keyNoteMapping[lastKey]))
                    {
                        actionDict[ActionTypes.KeyUp].actionEvent.Invoke(numberOctaveMapping[lastNum] * 5 + keyNoteMapping[lastKey]);
                        lastNum = Key.None;
                        lastKey = Key.None;

                    }
                    else if (heldNotes.Add(numberOctaveMapping[lastNum] * 5 + keyNoteMapping[lastKey]))
                    {
                        actionDict[ActionTypes.KeyDown].actionEvent.Invoke(numberOctaveMapping[lastNum] * 5 + keyNoteMapping[lastKey]);
                        lastNum = Key.None;
                        lastKey = Key.None;
                    }
                }
            }
            else
            {
                foreach (var num in numberOctaveMapping.Keys)
                {
                    var keyControl = Keyboard.current[num];
                    if (keyControl.wasPressedThisFrame && Enum.TryParse(keyControl.keyCode.ToString(), out Key heldNum))
                        lastNum = heldNum;
                    else if (keyControl.wasReleasedThisFrame && lastNum == num)
                        lastNum = Key.None;
                }
                foreach (var key in keyNoteMapping.Keys)
                {
                    var keyControl = Keyboard.current[key];
                    if (keyControl.wasPressedThisFrame && Enum.TryParse(keyControl.keyCode.ToString(), out Key heldKey))
                        lastKey = heldKey;
                    else if (keyControl.wasReleasedThisFrame && lastNum == key)
                        lastKey = Key.None;
                }

                if (lastNum != Key.None && lastKey != Key.None)
                {
                    if (heldNote != numberOctaveMapping[lastNum] * 5 + keyNoteMapping[lastKey])
                    {
                        if(heldNote > 0)
                            actionDict[ActionTypes.KeyUp].actionEvent.Invoke(heldNote);
                            
                        actionDict[ActionTypes.KeyDown].actionEvent.Invoke(numberOctaveMapping[lastNum] * 5 + keyNoteMapping[lastKey]);
                        heldNote = numberOctaveMapping[lastNum] * 5 + keyNoteMapping[lastKey];
                    }
                }
                else if (heldNote > 0)
                {
                    actionDict[ActionTypes.KeyUp].actionEvent.Invoke(heldNote);
                    heldNote = 0;
                }
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

                    if (!keyToggle)
                    {
                        if (heldNote > 0)
                        {
                            actionDict[ActionTypes.KeyUp].actionEvent.Invoke(heldNote);
                            heldNote = 0;
                        }

                        lastKey = Key.None;
                        lastNum = Key.None;
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
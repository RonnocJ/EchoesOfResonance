using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PzNote
{
    private static readonly Dictionary<string, int> _noteOffsets = new Dictionary<string, int>
    {
        {"C", 0}, {"C#", 1},
        {"D", 2}, {"D#", 3},
        {"E", 4},
        {"F", 5}, {"F#", 6},
        {"G", 7}, {"G#", 8},
        {"A", 9}, {"A#", 10},
        {"B", 11}
    };
    public float Pitch
    {
        get => _pitch;
        set
        {
            if (value != _pitch)
            {
                _pitch = ((value + 24) % 25) + 1;
            }
        }
    }
    [SerializeField, Range(1, 25)]
    private float _pitch = 13;
    public string Name
    {
        get
        {
            int noteIndex = (int)((Pitch - 1) % 12);
            int octave = (int)((Pitch - 1) / 12) + 1;

            string pitch = _noteOffsets.FirstOrDefault(x => x.Value == noteIndex).Key;

            return $"{pitch}{octave}";
        }
    }
    public PzNote(float newPitch)
    {
        Pitch = newPitch;
    }
    public PzNote(string newName)
    {
        string notePart = new string(newName.TakeWhile(c => !char.IsDigit(c)).ToArray());
        string octavePart = new string(newName.SkipWhile(c => !char.IsDigit(c)).ToArray());

        if (!_noteOffsets.TryGetValue(notePart, out int noteOffset))
            throw new ArgumentException($"Invalid note name: {newName}");

        if (!int.TryParse(octavePart, out int octave))
            throw new ArgumentException($"Invalid octave in note name: {newName}");

        float pitch = (octave - 1) * 12 + noteOffset + 1;

        Pitch = pitch;
    }
    public override bool Equals(object obj)
    {
        if (obj is not PzNote other) return false;
        return Mathf.Approximately(Pitch, other.Pitch);
    }

    public override int GetHashCode()
    {
        return Pitch.GetHashCode();
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(PzNote))]
public class PzNoteDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty pitchProp = property.FindPropertyRelative("_pitch");
        float pitch = pitchProp.floatValue;

        EditorGUI.BeginProperty(position, label, property);


        EditorGUI.LabelField(position, new PzNote(pitch).Name, EditorStyles.boldLabel);

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.MouseDown:
                if (position.Contains(evt.mousePosition))
                {
                    GUIUtility.hotControl = controlId;
                    evt.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlId)
                {
                    float delta = -evt.delta.y * 0.65f;
                    pitchProp.floatValue = ((pitch + delta + 24) % 25) + 1;
                    pitchProp.floatValue = Mathf.RoundToInt(pitchProp.floatValue);
                    property.serializedObject.ApplyModifiedProperties();
                    evt.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlId)
                {
                    GUIUtility.hotControl = 0;
                    evt.Use();
                }
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 20f;
}

#endif
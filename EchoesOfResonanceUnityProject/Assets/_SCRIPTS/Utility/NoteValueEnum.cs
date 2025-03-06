using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Enum defining note values as fractions of a whole note
public enum NoteValueEnum
{
    Whole = 16,
    DottedHalf = 12,
    Half = 8,
    DottedQuarter = 6,
    Quarter = 4,
    DottedEighth = 3,
    Eighth = 2,
    Sixteenth = 1
}

// Class that stores the accumulated note value
[Serializable]
public class NoteValue
{
    public static readonly Dictionary<NoteValueEnum, string> NoteUnicodeChars = new()
    {
        { NoteValueEnum.Whole, "w" },
        { NoteValueEnum.DottedHalf, "h." },
        { NoteValueEnum.Half, "h" },
        { NoteValueEnum.DottedQuarter, "q." },
        { NoteValueEnum.Quarter, "q" },
        { NoteValueEnum.DottedEighth, "e." },
        { NoteValueEnum.Eighth, "e" },
        { NoteValueEnum.Sixteenth, "x" }
    };
    public float CurrentValue;
}

[CustomPropertyDrawer(typeof(NoteValue))]
public class NoteValueDrawer : PropertyDrawer
{
    private static NoteValuePopup _popup;

    private static Font _musicFont;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_musicFont == null)
        {
            _musicFont = Font.CreateDynamicFontFromOSFont("Opus", 20);
        }

        SerializedProperty valueProp = property.FindPropertyRelative("CurrentValue");
        float currentValue = valueProp.floatValue;
        string displayText = GetOptimalNoteRepresentation(currentValue);

        GUIStyle iconStyle = new GUIStyle(EditorStyles.popup)
        {
            font = _musicFont,
            fontSize = 16,
            fixedHeight = 25,
            alignment = TextAnchor.MiddleLeft
        };

        Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, 25);

        EditorGUI.LabelField(labelRect, label);

        if (GUI.Button(buttonRect, displayText, iconStyle))
        {
            NoteValuePopup.Show(position, property);
        }     
    }

    private string GetOptimalNoteRepresentation(float value)
    {
        List<string> result = new();
        foreach (var note in NoteValue.NoteUnicodeChars)
        {
            while (value >= (float)note.Key / 4)
            {
                value -= (float)note.Key / 4;
                result.Add(note.Value);
            }
        }
        return result.Count > 0 ? string.Join(" + ", result) : "";
    }
}

public class NoteValuePopup : EditorWindow
{
    private static SerializedProperty _property;
    private static Font _musicFont;

    public static void Show(Rect activatorRect, SerializedProperty property)
    {
        _property = property;
        NoteValuePopup window = CreateInstance<NoteValuePopup>();
        window.ShowAsDropDown(GUIUtility.GUIToScreenRect(activatorRect), new Vector2(150, 305));

    }

    private void OnGUI()
    {
        if (_musicFont == null)
        {
            _musicFont = Font.CreateDynamicFontFromOSFont("Opus", 20);
        }

        SerializedProperty valueProp = _property.FindPropertyRelative("CurrentValue");

        GUIStyle fontStyle = new GUIStyle(GUI.skin.button)
        {
            font = _musicFont,
            fontSize = 16
        };

        foreach (var note in NoteValue.NoteUnicodeChars)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(note.Value, fontStyle);

            if (GUILayout.Button("+"))
            {
                valueProp.floatValue += (float)note.Key / 4;
                valueProp.serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("-"))
            {
                valueProp.floatValue = Mathf.Max(0, valueProp.floatValue - (float)note.Key / 4);
                valueProp.serializedObject.ApplyModifiedProperties();
            }

            GUILayout.EndHorizontal();
        }
    }
}

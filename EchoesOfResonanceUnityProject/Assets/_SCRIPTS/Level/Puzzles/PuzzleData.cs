using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum PuzzleType
{
    Solveable,
    Repeatable
}
[CreateAssetMenu(menuName = "Objects/Puzzles/PuzzleData", order = 0)]
public class PuzzleData : ScriptableObject
{
    public PuzzleType puzzleType;
    [Serializable]
    public class Notes
    {
        public PzNote note;
        public NoteValue noteDuration;
        public bool checkpoint;
    }
    public Notes[] solutions;
    public bool Active;
    public bool Solved
    {
        get =>
        puzzleType != PuzzleType.Repeatable && solved == solutions.Length;
    }
    [SerializeField] private int _solved;
    public int solved
    {
        get => _solved;
        set
        {
            if (value == solutions.Length && Active)
            {
                OnPuzzleCompleted?.Invoke();
                BrPitchFinder.CheckFinder?.Invoke();
            }
            
            if (_solved != value && Active)
            {
                OnSolvedChanged?.Invoke(value);
            }

            _solved = value;
        }
    }
    [SerializeField] private int _reset;
    public int reset
    {
        get => _reset;
        set
        {
            _reset = value;
            if (value == 3)
            {
                OnReset?.Invoke();
                BrPitchFinder.CheckFinder?.Invoke();
            }
        }
    }
    public AudioEffects[] audioEffects;
    public Action<int> OnSolvedChanged;
    public Action OnPuzzleCompleted;
    public Action OnReset;
    public void SetMusicComplete()
    {
        OnPuzzleCompleted += MusicManager.root.RefreshMusicData;
        if (audioEffects != null)
        {
            foreach (var effect in audioEffects)
            {
                if (effect.executeOnSolved)
                {
                    OnPuzzleCompleted += effect.ExecuteActions;
                }
                else
                {
                    OnSolvedChanged += c => { if (c == effect.executeEarly) { effect.ExecuteActions(); } };
                }
            }
        }
    }
    public int FindLastCheckpoint(int iEnd = -1)
    {
        if (iEnd == -1) iEnd = solved;

        for (int i = iEnd; i >= 0; i--)
        {
            if (solutions[i].checkpoint)
            {
                return i;
            }
        }
        return 0;
    }

}
#if UNITY_EDITOR 
[CustomPropertyDrawer(typeof(PuzzleData.Notes))]
public class NotesPropertyDrawer : PropertyDrawer
{
    static public bool Checkpoint;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect noteRect = new Rect(position.x, position.y + 2.5f, 50, EditorGUIUtility.singleLineHeight);
        Rect durationRect = new Rect(noteRect.xMax - 125, position.y, 250, EditorGUIUtility.singleLineHeight);
        Rect checkpointLabelRect = new Rect(durationRect.xMax + 80, position.y + 2.5f, 100, EditorGUIUtility.singleLineHeight);
        Rect checkpointRect = new Rect(checkpointLabelRect.xMax - 20, position.y + 2.5f, 25, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(noteRect, property.FindPropertyRelative("note"), GUIContent.none);
        EditorGUI.PropertyField(durationRect, property.FindPropertyRelative("noteDuration"), GUIContent.none);
        EditorGUI.LabelField(checkpointLabelRect, new GUIContent("Checkpoint:"));
        EditorGUI.PropertyField(checkpointRect, property.FindPropertyRelative("checkpoint"), GUIContent.none);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty solutionsArray = property.serializedObject.FindProperty("solutions");

        int index = GetArrayIndex(property, solutionsArray);

        float extraHeight = 6f;

        if (index < solutionsArray.arraySize && solutionsArray.GetArrayElementAtIndex(index).FindPropertyRelative("checkpoint").boolValue)
        {
            extraHeight = 35f;
        }

        return EditorGUIUtility.singleLineHeight + extraHeight;
    }

    private int GetArrayIndex(SerializedProperty property, SerializedProperty array)
    {
        for (int i = 0; i < array.arraySize; i++)
        {
            if (property.propertyPath == $"{array.propertyPath}.Array.data[{i}]")
            {
                return i + 1;
            }
        }
        return -1;
    }
}
#endif
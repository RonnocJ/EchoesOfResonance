using System;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/PuzzleData", order = 0)]
public class PuzzleData : ScriptableObject
{
    [Serializable]
    public class Notes
    {
        public string noteName;
        public NoteValue noteDuration;
        public bool checkpoint;
    }
    public Notes[] solutions;
    [SerializeField] private int _solved;
    public int solved
    {
        get => _solved;
        set
        {
            if (_solved != value) OnSolvedChanged.Invoke(value);
            _solved = value;
            if (value == solutions.Length) OnPuzzleCompleted.Invoke();
        }
    }
    [SerializeField] private int _reset;
    public int reset
    {
        get => _reset;
        set
        {
            _reset = value;
            if (value == 3) OnReset.Invoke();
        }
    }
    public AudioEffects[] audioEffects;

    public Action<int> OnSolvedChanged;
    public Action OnPuzzleCompleted;
    public Action OnReset;

    public void SetMusicComplete()
    {
        if (audioEffects != null)
        {
            foreach (var effect in audioEffects)
            {
                if (effect.executeOnSolved)
                {
                    OnPuzzleCompleted += () => ExecuteActions(effect);
                }
                else
                {
                    OnSolvedChanged += c => { if (c == effect.executeEarly) ExecuteActions(effect); };
                }
            }
        }
    }

    public void ExecuteActions(AudioEffects effect)
    {
        if ((effect.audioTypes & AudioEffectType.Event) != 0)
        {
            foreach (var e in effect.audioEvents)
            {
                AudioManager.root.PlaySound(e, MusicManager.root.gameObject);
            }
        }
        if ((effect.audioTypes & AudioEffectType.State) != 0)
        {
            foreach (var st in effect.audioStates)
            {
                MusicManager.root.SetState(st);
            }
        }
        if ((effect.audioTypes & AudioEffectType.Switch) != 0)
        {
            foreach (var sw in effect.audioSwitches)
            {
                AudioManager.root.SetSwitch(sw, MusicManager.root.gameObject);
            }
        }
        if ((effect.audioTypes & AudioEffectType.Trigger) != 0)
        {
            foreach (var t in effect.audioTriggers)
            {
                MusicManager.root.SetTrigger(t);
            }
        }
        if ((effect.audioTypes & AudioEffectType.RTPC) != 0)
        {
            foreach (var r in effect.audioRTPCs)
            {
                AudioManager.root.SetRTPC(r.parameter, r.value);
            }
        }

    }
}

[CustomPropertyDrawer(typeof(PuzzleData.Notes))]
public class NotesPropertyDrawer : PropertyDrawer
{
    static public bool Checkpoint;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty solutionsArray = property.serializedObject.FindProperty("solutions");

        int index = GetArrayIndex(property, solutionsArray);

        SerializedProperty noteNameProp = property.FindPropertyRelative("noteName");
        SerializedProperty noteDurationProp = property.FindPropertyRelative("noteDuration");
        SerializedProperty checkpointProp = property.FindPropertyRelative("checkpoint");

        Rect indexRect = new Rect(position.x, position.y, 25f, 20f);
        EditorGUI.LabelField(indexRect, $"{index}:");

        Rect noteRect = new Rect(position.x + 25f, position.y + 1f, 40f, 20f);
        Rect durationRect = new Rect(noteRect.x - 75, position.y, 250f, 30f);
        Rect checkpointRect = new Rect(durationRect.x + 275, position.y, 25f, 25f);

        EditorGUI.PropertyField(noteRect, noteNameProp, GUIContent.none);
        EditorGUI.PropertyField(durationRect, noteDurationProp, GUIContent.none);
        EditorGUI.PropertyField(checkpointRect, checkpointProp, new GUIContent("Checkpoint:"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty solutionsArray = property.serializedObject.FindProperty("solutions");

        int index = GetArrayIndex(property, solutionsArray);

        float extraHeight = 6f;

        if(index < solutionsArray.arraySize && solutionsArray.GetArrayElementAtIndex(index).FindPropertyRelative("checkpoint").boolValue)
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

/*[CustomEditor(typeof(PuzzleData))]
public class PuzzleDataEditor : Editor
{
    private SerializedProperty solutionsProp;
    private SerializedProperty checkpointsProp;
    private Vector2 scrollPos; // For scrolling when lists get too long

    private void OnEnable()
    {
        solutionsProp = serializedObject.FindProperty("solutions");
        checkpointsProp = serializedObject.FindProperty("checkpoints");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Puzzle Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(solutionsProp, new GUIContent("Solutions:"));
        EditorGUILayout.Space(10);

        EditorGUILayout.PropertyField(checkpointsProp, new GUIContent("Checkpoints:"), GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.4f));
        EditorGUILayout.Space(10);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true);

        while (property.NextVisible(false))
        {
            if (property.name is "solutions" or "checkpoints")
            {
                continue;
            }

            EditorGUILayout.PropertyField(property, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
*/
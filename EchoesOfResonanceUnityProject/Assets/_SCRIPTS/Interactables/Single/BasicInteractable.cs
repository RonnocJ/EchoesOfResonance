using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
public abstract class BasicInteractable : MonoBehaviour
{
    [SerializeField] private bool linkedWithPuzzle;
    public bool IsLinkedWithPuzzle
    {
        get => linkedWithPuzzle;
        private set => linkedWithPuzzle = value;
    }
    [SerializeField] private PuzzleData linkedData;

    public PuzzleData LinkedData
    {
        get => linkedData;
        set => linkedData = value;
    }
    [SerializeField] private bool executeOnFinish;
    public bool ExecuteOnFinish
    {
        get => executeOnFinish;
        private set => executeOnFinish = value;
    }
    [SerializeField] private int executeEarly;
    public int ExecuteEarly
    {
        get => executeEarly;
        private set => executeEarly = value;
    }
    [SerializeField] private bool timedReset;
    [SerializeField] private float resetDelay;
    private static Dictionary<GameObject, bool> _activated = new();
    public virtual void Awake()
    {
        _activated[gameObject] = false;

        if (IsLinkedWithPuzzle)
        {
            if (executeOnFinish)
            {
                linkedData.OnPuzzleCompleted += () => ActivateObject();
            }
            else
            {
                linkedData.OnSolvedChanged += solved =>
                {
                    if (solved >= executeEarly && !_activated[gameObject]) ActivateObject();
                    else if (_activated[gameObject] && solved < executeEarly) ResetObject();
                };
            }
        }
    }
    public virtual void ActivateObject()
    {
        _activated[gameObject] = true;

        if (timedReset) Invoke(nameof(ResetObject), resetDelay);
    }
    public virtual void ResetObject()
    {
        _activated[gameObject] = false;
    }
}
#if UNITY_EDITOR 
[CustomEditor(typeof(BasicInteractable), true)]
public class BasicInteractableEditor : Editor
{
    SerializedProperty linkedWithPuzzle;
    SerializedProperty linkedData;
    SerializedProperty executeOnFinish;
    SerializedProperty executeEarly;
    void OnEnable()
    {
        linkedWithPuzzle = serializedObject.FindProperty("linkedWithPuzzle");
        linkedData = serializedObject.FindProperty("linkedData");
        executeOnFinish = serializedObject.FindProperty("executeOnFinish");
        executeEarly = serializedObject.FindProperty("executeEarly");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((BasicInteractable)target), typeof(MonoScript), false);
        GUI.enabled = true;

        EditorGUILayout.PropertyField(linkedWithPuzzle, new GUIContent("Is linked with puzzle"));

        if (linkedWithPuzzle.boolValue)
        {
            EditorGUILayout.PropertyField(linkedData, new GUIContent("Linked Puzzle Data:"));
            EditorGUILayout.PropertyField(executeOnFinish, new GUIContent("Execute on solved"));

            if (!executeOnFinish.boolValue)
            {
                EditorGUILayout.PropertyField(executeEarly, new GUIContent("Execute on index:"));
            }
        }

        EditorGUILayout.Space();

        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true);

        while (property.NextVisible(false))
        {
            if (property.name == "linkedWithPuzzle" || property.name == "linkedData" ||
                property.name == "executeOnFinish" || property.name == "executeEarly")
            {
                continue;
            }

            EditorGUILayout.PropertyField(property, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
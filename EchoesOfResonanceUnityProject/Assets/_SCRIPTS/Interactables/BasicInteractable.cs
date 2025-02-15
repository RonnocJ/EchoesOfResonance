using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BasicInteractable), true)]
public class BasicInteractableEditor : Editor
{
    SerializedProperty linkedWithPuzzle;
    SerializedProperty linkedData;
    SerializedProperty executeOnFinish;
    SerializedProperty executeEarly;
    SerializedProperty timedReset;
    SerializedProperty resetDelay;
    void OnEnable()
    {
        linkedWithPuzzle = serializedObject.FindProperty("linkedWithPuzzle");
        linkedData = serializedObject.FindProperty("linkedData");
        executeOnFinish = serializedObject.FindProperty("executeOnFinish");
        executeEarly = serializedObject.FindProperty("executeEarly");
        timedReset = serializedObject.FindProperty("timedReset");
        resetDelay = serializedObject.FindProperty("resetDelay");
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
    else
    {
        EditorGUILayout.PropertyField(timedReset, new GUIContent("Reset after a delay"));

        if (timedReset.boolValue)
        {
            EditorGUILayout.PropertyField(resetDelay, new GUIContent("Delay time:"));
        }
    }

    EditorGUILayout.Space();

    SerializedProperty property = serializedObject.GetIterator();
    property.NextVisible(true); 

    while (property.NextVisible(false))
    { 
        if (property.name == "linkedWithPuzzle" || property.name == "linkedData" ||
            property.name == "executeOnFinish" || property.name == "executeEarly" ||
            property.name == "timedReset" || property.name == "resetDelay")
        {
            continue;
        }

        EditorGUILayout.PropertyField(property, true);
    }

    serializedObject.ApplyModifiedProperties();
}

}
public abstract class BasicInteractable : MonoBehaviour
{
    [SerializeField] private bool linkedWithPuzzle;
    public bool IsLinkedWithPuzzle
    {
        get => linkedWithPuzzle;
        private set => linkedWithPuzzle = value;
    }
    [SerializeField] private PuzzleData linkedData; // Private for Unity serialization

    public PuzzleData LinkedData
    {
        get => linkedData;
        set => linkedData = value;
    }
    [SerializeField] private bool executeOnFinish;
    [SerializeField] private int executeEarly;
    [SerializeField] private bool timedReset;
    [SerializeField] private float resetDelay;
    private bool _activated;
    void Awake()
    {
        if (executeOnFinish)
        {
            linkedData.OnPuzzleCompleted += () => ActivateObject();
        }
        else if (executeEarly != 0 && executeEarly < linkedData.solutions.Length)
        {
            linkedData.OnSolvedChanged += solved =>
            {
                if (solved == executeEarly && !_activated) ActivateObject();
                else if (_activated && solved < executeEarly) ResetObject();
            };
        }
    }
    public virtual void ActivateObject()
    {
        _activated = true;

        if (timedReset) Invoke(nameof(ResetObject), resetDelay);
    }
    public virtual void ResetObject()
    {
        _activated = false;
    }
}
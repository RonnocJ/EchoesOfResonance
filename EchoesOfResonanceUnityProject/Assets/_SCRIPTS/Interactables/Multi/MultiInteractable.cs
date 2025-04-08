using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[Serializable]
public abstract class MultiInteractableStep
{
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
    [HideInInspector]
    public bool activated;
    [HideInInspector]
    public MultiInteractable parent;
    public virtual void ActivateObject()
    {
        activated = true;

        if (timedReset) CRManager.root.Restart(ResetAfterDelay(resetDelay), $"{parent.gameObject}ResetDelayStep{ExecuteEarly}", parent);
    }
    public virtual void ResetObject()
    {
        activated = false;
    }
    public virtual IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetObject();
    }
}
public abstract class MultiInteractable : MonoBehaviour
{
    [SerializeField] private PuzzleData linkedData;

    public PuzzleData LinkedData
    {
        get => linkedData;
        set => linkedData = value;
    }
    [SerializeReference] protected List<MultiInteractableStep> steps = new();

#if UNITY_EDITOR
    protected abstract Type GetStepType();
#endif

    public virtual void Awake()
    {
        foreach (var step in steps)
        {
            step.parent = this;

            if (step.ExecuteOnFinish)
            {
                linkedData.OnPuzzleCompleted += () => step.ActivateObject();
            }
            else
            {
                linkedData.OnSolvedChanged += solved =>
                {
                    if (solved >= step.ExecuteEarly && !step.activated) step.ActivateObject();
                };
            }
        }


        linkedData.OnSolvedChanged += solved =>
        {
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                if (!steps[i].ExecuteOnFinish)
                {
                    if (solved < steps[i].ExecuteEarly && steps[i].activated) steps[i].ResetObject();
                }
            }
        };
    }
}
#if UNITY_EDITOR 
[CustomEditor(typeof(MultiInteractableStep), true)]
public class MultiInteractableStepEditor : Editor
{
    SerializedProperty executeOnFinish;
    SerializedProperty executeEarly;

    void OnEnable()
    {
        executeOnFinish = serializedObject.FindProperty("executeOnFinish");
        executeEarly = serializedObject.FindProperty("executeEarly");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(executeOnFinish, new GUIContent("Execute On Finish"));

        if (executeOnFinish.boolValue == false)
        {
            EditorGUILayout.PropertyField(executeEarly, new GUIContent("Execute on index:"));
        }

        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true);

        while (property.NextVisible(false))
        {
            if (property.name == "executeOnFinish" || property.name == "executeEarly")
            {
                continue;
            }

            EditorGUILayout.PropertyField(property, true);
        }

        serializedObject.ApplyModifiedProperties();
        Repaint();
    }
}

[CustomEditor(typeof(MultiInteractable), true)]
public class MultiInteractableEditor : Editor
{
    private SerializedProperty stepsProperty;
    private Type stepType;

    private void OnEnable()
    {
        stepsProperty = serializedObject.FindProperty("steps");

        if (target is MultiInteractable interactable)
        {
            stepType = interactable.GetType().GetMethod("GetStepType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(interactable, null) as Type;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true);

        while (property.NextVisible(false))
        {
            if (property.name == "steps")
            {
                continue;
            }

            EditorGUILayout.PropertyField(property, true);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("MultiInteractable Steps", EditorStyles.boldLabel);

        if (stepType == null)
        {
            EditorGUILayout.HelpBox("No valid step type found.", MessageType.Warning);
            return;
        }

        for (int i = 0; i < stepsProperty.arraySize; i++)
        {
            SerializedProperty stepElement = stepsProperty.GetArrayElementAtIndex(i);

            if (stepElement.managedReferenceValue == null || stepElement.managedReferenceValue.GetType() != stepType)
            {
                stepsProperty.DeleteArrayElementAtIndex(i);
                continue;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(stepElement, new GUIContent($"Step {i + 1}"), true);

            if (GUILayout.Button("Remove Step"))
            {
                stepsProperty.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Step"))
        {
            stepsProperty.InsertArrayElementAtIndex(stepsProperty.arraySize);
            SerializedProperty newElement = stepsProperty.GetArrayElementAtIndex(stepsProperty.arraySize - 1);
            newElement.managedReferenceValue = Activator.CreateInstance(stepType);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
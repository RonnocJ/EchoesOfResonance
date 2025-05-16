using System;
using UnityEngine;
using UnityEditor;
[Serializable]
public class PhysicsMoveStep
{
    public float Time;
    public Vector3 BoostDir;
    public float BoostForce;
    public float ResetForce;
    public bool ChangeMass;
    public float NewMass;
    public bool ChangeDrag;
    public float NewDrag;
    public object parent;
    [HideInInspector]
    public bool Forwards;
    public void BoostObject<T>(float force) where T : MonoBehaviour
    {
        if (parent is T p)
        {
            var rb = p.GetComponent<Rigidbody>();

            rb.AddForce(force * BoostDir);
        }
    }
}
#if UNITY_EDITOR 
[CustomPropertyDrawer(typeof(PhysicsMoveStep))]
public class PhysicsMoveStepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Start property
        EditorGUI.BeginProperty(position, label, property);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = -1;

        Rect timeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect boostDirRect = new Rect(position.x, timeRect.yMax + 2, position.width, EditorGUIUtility.singleLineHeight);
        Rect boostForceRect = new Rect(position.x, boostDirRect.yMax + 2, position.width, EditorGUIUtility.singleLineHeight);
        Rect changeMassRect = new Rect(position.x, boostForceRect.yMax + 2, position.width, EditorGUIUtility.singleLineHeight);

        SerializedProperty timeProp = property.FindPropertyRelative("Time");
        SerializedProperty boostDirProp = property.FindPropertyRelative("BoostDir");
        SerializedProperty boostForceProp = property.FindPropertyRelative("BoostForce");
        SerializedProperty changeMassProp = property.FindPropertyRelative("ChangeMass");
        SerializedProperty newMassProp = property.FindPropertyRelative("NewMass");
        SerializedProperty changeDragProp = property.FindPropertyRelative("ChangeDrag");
        SerializedProperty newDragProp = property.FindPropertyRelative("NewDrag");

        EditorGUI.PropertyField(timeRect, timeProp);
        EditorGUI.PropertyField(boostDirRect, boostDirProp);
        EditorGUI.PropertyField(boostForceRect, boostForceProp);
        EditorGUI.PropertyField(changeMassRect, changeMassProp, new GUIContent("Change Mass"));

        Rect nextRect = new Rect(position.x, changeMassRect.yMax + 2, position.width, EditorGUIUtility.singleLineHeight);

        if (changeMassProp.boolValue)
        {
            EditorGUI.PropertyField(nextRect, newMassProp, new GUIContent("New Mass"));
            nextRect = new Rect(position.x, nextRect.yMax + 2, position.width, EditorGUIUtility.singleLineHeight);
        }

        EditorGUI.PropertyField(nextRect, changeDragProp, new GUIContent("Change Drag"));
        nextRect = new Rect(position.x, nextRect.yMax + 2, position.width, EditorGUIUtility.singleLineHeight);

        if (changeDragProp.boolValue)
        {
            EditorGUI.PropertyField(nextRect, newDragProp, new GUIContent("New Drag"));
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 4;

        if (property.FindPropertyRelative("ChangeMass").boolValue)
            lines++;
        lines++;
        if (property.FindPropertyRelative("ChangeDrag").boolValue)
            lines++;

        return lines * (EditorGUIUtility.singleLineHeight + 2);
    }
}
#endif
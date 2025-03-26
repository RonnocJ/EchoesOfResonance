using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[Serializable]
public struct TrData
{
    [Flags]
    public enum IncludeInMove
    {
        None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }

    public IncludeInMove EffectedProperties;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public TrData(Vector3? newPos = null, Quaternion? newRot = null, Vector3? newScale = null)
    {
        position = newPos ?? Vector3.zero;
        rotation = newRot?.eulerAngles ?? Vector3.zero;
        scale = newScale ?? Vector3.one;

        EffectedProperties = 0;
        if (newPos != null) EffectedProperties |= IncludeInMove.Position;
        if (newRot != null) EffectedProperties |= IncludeInMove.Rotation;
        if (newScale != null) EffectedProperties |= IncludeInMove.Scale;
    }

    public TrData(Transform tr, IncludeInMove include = IncludeInMove.Position | IncludeInMove.Rotation | IncludeInMove.Scale)
    {
        EffectedProperties = include;

        position = (include & IncludeInMove.Position) != 0 ? tr.localPosition : Vector3.zero;
        rotation = (include & IncludeInMove.Rotation) != 0 ? tr.localRotation.eulerAngles : Vector3.zero;
        scale = (include & IncludeInMove.Scale) != 0 ? tr.localScale : Vector3.one;
    }

    public void ApplyTo(Transform tr)
    {
        if ((EffectedProperties & IncludeInMove.Position) != 0)
            tr.localPosition = position;
        if ((EffectedProperties & IncludeInMove.Rotation) != 0)
            tr.localRotation = Quaternion.Euler(rotation);
        if ((EffectedProperties & IncludeInMove.Scale) != 0)
            tr.localScale = scale;
    }

    public IEnumerator ApplyToOverTime(Transform tr, float duration, AnimationCurve curve = null, AudioEvent startSound = AudioEvent.None, AudioEvent movingSound = AudioEvent.None, AudioEvent finishSound = AudioEvent.None)
    {
        if (curve == null) curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        Vector3 startPos = tr.localPosition;
        Quaternion startRot = tr.localRotation;
        Vector3 startScale = tr.localScale;

        float elapsed = 0f;
        
        AudioManager.root.PlaySound(startSound, tr.gameObject);
        AudioManager.root.PlaySound(movingSound, tr.gameObject);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));

            if ((EffectedProperties & IncludeInMove.Position) != 0)
                tr.localPosition = Vector3.Lerp(startPos, position, t);
            if ((EffectedProperties & IncludeInMove.Rotation) != 0)
                tr.localRotation = Quaternion.Lerp(startRot, Quaternion.Euler(rotation), t);
            if ((EffectedProperties & IncludeInMove.Scale) != 0)
                tr.localScale = Vector3.Lerp(startScale, scale, t);

            yield return null;
        }

        ApplyTo(tr);
        AudioManager.root.StopSound(movingSound, tr.gameObject);
        AudioManager.root.PlaySound(finishSound, tr.gameObject);
    }

    public bool IsEqualTo(Transform tr)
    {
        if ((EffectedProperties & IncludeInMove.Position) != 0 && tr.localPosition != position)
            return false;
        if ((EffectedProperties & IncludeInMove.Rotation) != 0 && tr.rotation != Quaternion.Euler(rotation))
            return false;
        if ((EffectedProperties & IncludeInMove.Scale) != 0 && tr.localScale != scale)
            return false;
        return true;
    }
}
[Serializable]
public struct SaveStruct
{
    public float[] position;
    public float[] rotation;
    public float[] scale;

    public SaveStruct(TrData trData)
    {
        position = new float[] { trData.position.x, trData.position.y, trData.position.z };
        rotation = new float[] { trData.rotation.x, trData.rotation.y, trData.rotation.z };
        scale = new float[] {trData.scale.x, trData.scale.y, trData.scale.z};
    }

    public TrData LoadData()
    {
        return new TrData (
            new Vector3(position[0], position[1], position[2]), 
            Quaternion.Euler(new Vector3(rotation[0], rotation[1], rotation[2])), 
            new Vector3(scale[0], scale[1], scale[2])
        );
    }
}

[CustomPropertyDrawer(typeof(TrData))]
public class TrDataDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty includeInMove = property.FindPropertyRelative("EffectedProperties");
        float fieldCount = 2;

        if ((includeInMove.intValue & (int)TrData.IncludeInMove.Position) != 0) fieldCount += 1.125f;
        if ((includeInMove.intValue & (int)TrData.IncludeInMove.Rotation) != 0) fieldCount += 1.125f;
        if ((includeInMove.intValue & (int)TrData.IncludeInMove.Scale) != 0) fieldCount += 1.125f;

        return EditorGUIUtility.singleLineHeight * fieldCount;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty includeInMove = property.FindPropertyRelative("EffectedProperties");

        Rect lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        includeInMove.intValue = (int)(TrData.IncludeInMove)EditorGUI.EnumFlagsField(lineRect, "Include In Move", (TrData.IncludeInMove)includeInMove.intValue);

        lineRect.y += EditorGUIUtility.singleLineHeight;

        if ((includeInMove.intValue & (int)TrData.IncludeInMove.Position) != 0)
        {
            SerializedProperty pos = property.FindPropertyRelative("position");
            lineRect.y += EditorGUIUtility.singleLineHeight * 1.125f;
            EditorGUI.PropertyField(lineRect, pos, new GUIContent("Position: "));
        }
        if ((includeInMove.intValue & (int)TrData.IncludeInMove.Rotation) != 0)
        {
            SerializedProperty rot = property.FindPropertyRelative("rotation");
            lineRect.y += EditorGUIUtility.singleLineHeight * 1.125f;
            EditorGUI.PropertyField(lineRect, rot, new GUIContent("Rotation: "));
        }
        if ((includeInMove.intValue & (int)TrData.IncludeInMove.Scale) != 0)
        {
            SerializedProperty scale = property.FindPropertyRelative("scale");
            lineRect.y += EditorGUIUtility.singleLineHeight * 1.125f;
            EditorGUI.PropertyField(lineRect, scale, new GUIContent("Scale: "));
        }

        EditorGUI.EndProperty();
    }
}

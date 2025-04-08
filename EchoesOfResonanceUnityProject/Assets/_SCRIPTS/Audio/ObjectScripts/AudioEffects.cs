using UnityEngine;
using UnityEditor;
using System;
using AK.Wwise;
[Flags]
public enum AudioEffectType
{
    Event = 1 << 0,
    State = 1 << 1,
    Switch = 1 << 2,
    Trigger = 1 << 3,
    RTPC = 1 << 4
}
    [Serializable]
    public class RTPC
    {
        public AudioRTPC parameter;
        public float value;
    }
[Serializable]
public class AudioEffects
{
    public bool executeOnSolved;
    public int executeEarly;
    public AudioEffectType audioTypes;
    public AudioEvent[] audioEvents;
    public AudioState[] audioStates;
    public AudioSwitch[] audioSwitches;
    public AudioTrigger[] audioTriggers;
    public RTPC[] audioRTPCs;
}
#if UNITY_EDITOR 
[CustomPropertyDrawer(typeof(AudioEffects))]
public class AudioEffectsEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty onSolved = property.FindPropertyRelative("executeOnSolved");
        SerializedProperty early = property.FindPropertyRelative("executeEarly");
        SerializedProperty audioTypes = property.FindPropertyRelative("audioTypes");
        SerializedProperty audioEvents = property.FindPropertyRelative("audioEvents");
        SerializedProperty audioStates = property.FindPropertyRelative("audioStates");
        SerializedProperty audioSwitches = property.FindPropertyRelative("audioSwitches");
        SerializedProperty audioTriggers = property.FindPropertyRelative("audioTriggers");
        SerializedProperty audioRTPCs = property.FindPropertyRelative("audioRTPCs");

        string key = "AudioEffects_" + property.propertyPath;
        if (!EditorPrefs.HasKey(key)) EditorPrefs.SetInt(key, 1);

        if (property.isExpanded == false && !EditorPrefs.HasKey(key))
        {
            property.isExpanded = true;
            EditorPrefs.SetInt(key, 1);
        }

        // Create a foldout
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded, label);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

             EditorGUILayout.PropertyField(onSolved, new GUIContent("Execute on Solved"));

            if (!onSolved.boolValue)
            {
                EditorGUILayout.PropertyField(early, new GUIContent("Execute on index:"));
            }

            audioTypes.intValue = (int)(AudioEffectType)EditorGUILayout.EnumFlagsField("Audio Effect Type", (AudioEffectType)audioTypes.intValue);
            AudioEffectType selectedTypes = (AudioEffectType)audioTypes.intValue;

            if (selectedTypes.HasFlag(AudioEffectType.Event))
            {
                EditorGUILayout.PropertyField(audioEvents, new GUIContent("Events:"));
            }
            if (selectedTypes.HasFlag(AudioEffectType.State))
            {
                EditorGUILayout.PropertyField(audioStates, new GUIContent("States:"));
            }
            if (selectedTypes.HasFlag(AudioEffectType.Switch))
            {
                EditorGUILayout.PropertyField(audioSwitches, new GUIContent("Switches:"));
            }
            if (selectedTypes.HasFlag(AudioEffectType.Trigger))
            {
                EditorGUILayout.PropertyField(audioTriggers, new GUIContent("Triggers:"));
            }
            if (selectedTypes.HasFlag(AudioEffectType.RTPC))
            {
                EditorGUILayout.PropertyField(audioRTPCs, new GUIContent("RTPCs:"));
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
#endif
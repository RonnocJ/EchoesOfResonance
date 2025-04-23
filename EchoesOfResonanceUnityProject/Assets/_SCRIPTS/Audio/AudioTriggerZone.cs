using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR 
[CustomEditor(typeof(AudioTriggerZone))]
public class AudioTriggerZoneEditor : Editor
{
    SerializedProperty singleTrigger;
    SerializedProperty types;
    SerializedProperty audioEvents;
    SerializedProperty audioStates;
    SerializedProperty audioSwitches;
    SerializedProperty audioTriggers;
    SerializedProperty audioRTPCs;
    void OnEnable()
    {
        singleTrigger = serializedObject.FindProperty("singleTrigger");
        types = serializedObject.FindProperty("effects");
        audioEvents = serializedObject.FindProperty("events");
        audioStates = serializedObject.FindProperty("states");
        audioSwitches = serializedObject.FindProperty("switches");
        audioTriggers = serializedObject.FindProperty("triggers");
        audioRTPCs = serializedObject.FindProperty("rtpcs");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        singleTrigger.boolValue = EditorGUILayout.Toggle("Single Trigger:", singleTrigger.boolValue);

        types.intValue = (int)(AudioEffectType)EditorGUILayout.EnumFlagsField("Audio Effect Type", (AudioEffectType)types.intValue);
        AudioEffectType selectedTypes = (AudioEffectType)types.intValue;

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

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
public class AudioTriggerZone : MonoBehaviour
{
    [SerializeField] private bool singleTrigger;
    [SerializeField] private AudioEffectType effects;
    [SerializeField] private AudioEvent[] events;
    [SerializeField] private AudioState[] states;
    [SerializeField] private AudioSwitch[] switches;
    [SerializeField] private AudioTrigger[] triggers;
    [SerializeField] private RTPC[] rtpcs;
    private bool triggered;
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && ((singleTrigger && !triggered) || !singleTrigger))
        {
            foreach (var e in events)
            {
                AudioManager.root.PlaySound(e);
            }
            foreach(var st in states)
            {
                MusicManager.root.SetState(st);
            }
            foreach (var sw in switches)
            {
                AudioManager.root.SetSwitch(sw);
            }
            foreach (var t in triggers)
            {
                MusicManager.root.SetTrigger(t);
            }
            foreach (var r in rtpcs)
            {
                AudioManager.root.SetRTPC(r.parameter, r.value);
            }

            if(singleTrigger) triggered = true;
        }
    }
}
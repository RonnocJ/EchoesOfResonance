using AK.Wwise;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR 
[CustomEditor(typeof(MusicRTPCZone))]
public class SetMinAndMax : Editor
{
    void OnSceneGUI()
    {
        var t = target as MusicRTPCZone;
        Handles.color = Color.green;
        Handles.DrawSolidDisc(t.minPos, Vector3.up, 1f);
        Handles.DrawSolidDisc(t.maxPos, Vector3.up, 1f);
    }
}
#endif
public class MusicRTPCZone : MonoBehaviour
{
    [SerializeField] private AudioRTPC controlRTPC;
    [SerializeField] private AnimationCurve interpolationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public Vector3 minPos, maxPos;

    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            float totalDist = Vector3.Distance(minPos, maxPos);
            float minDist = Vector3.Distance(minPos, col.transform.position);

            AudioManager.root.SetRTPC(controlRTPC, interpolationCurve.Evaluate(Mathf.Clamp01(minDist / totalDist)) * 100);
        }
    }
}
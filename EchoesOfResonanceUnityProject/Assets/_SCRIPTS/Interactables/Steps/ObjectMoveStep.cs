using System;
using System.Collections;
using UnityEngine;
[Serializable]
public class ObjectMoveStep
{
    public float shakeTime, moveTime;
    public TrData target;
    [HideInInspector]
    public object parent;

    public IEnumerator MoveObject<T>(AnimationCurve interpolationCurve, AudioEvent startSound = AudioEvent.None) where T : MonoBehaviour
    {
        if (parent is T p)
        {
            if (shakeTime > 0)
            {
                Vector3 originalPos = p.transform.position;
                float elapsed = 0f;

                while (elapsed < shakeTime)
                {
                    p.transform.position += UnityEngine.Random.insideUnitSphere * 0.05f;
                    yield return null;

                    p.transform.position = originalPos;
                    elapsed += Time.deltaTime;
                }
            }

            CRManager.Restart(target.ApplyToOverTime(p.transform, moveTime, interpolationCurve, (target.IsEqualTo(p.transform) || target.EffectedProperties == 0)? AudioEvent.None : startSound ), $"{p.gameObject}ObjMoveStep", p);
            yield return new WaitForSeconds(moveTime);
        }

    }
}
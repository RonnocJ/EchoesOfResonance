using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ObjectMoveStep
{
    public float shakeTime, moveTime;
    public TrData target;
    [HideInInspector]
    public MultiMoveableObject parent;

    public IEnumerator MoveObject()
    {
        if (shakeTime > 0)
        {
            Vector3 originalPos = parent.transform.position;
            float elapsed = 0f;

            while (elapsed < shakeTime)
            {
                parent.transform.position += UnityEngine.Random.insideUnitSphere * 0.05f;
                yield return null;

                parent.transform.position = originalPos;
                elapsed += Time.deltaTime;
            }
        }

        CRManager.root.Restart(target.ApplyToOverTime(parent.transform, moveTime), $"{parent.gameObject}ObjMoveStep", parent);
        yield return new WaitForSeconds(moveTime);
    }
}
public class MultiMoveStep : MultiInteractableStep
{
    public ObjectMoveStep[] steps;
    [HideInInspector]
    public override void ActivateObject()
    {
        base.ActivateObject();
        var multiParent = parent as MultiMoveableObject;

        if (multiParent != null)
        {
            foreach (var step in steps)
            {
                step.parent = multiParent;
                multiParent.stepQueue.Enqueue(step);
            }

            CRManager.root.Begin(multiParent.DepleteMoveQueue(), $"{multiParent.gameObject}DepleteMoveQueue", multiParent);
        }
    }

    public override void ResetObject()
    {
        base.ResetObject();
        var multiParent = parent as MultiMoveableObject;

        if (multiParent != null)
        {
            for (int i = steps.Length - 1; i >= 0; i--)
            {
                multiParent.stepQueue.Enqueue(steps[i]);
            }

            CRManager.root.Begin(multiParent.DepleteMoveQueue(), $"{multiParent.gameObject}DepleteMoveQueue", multiParent);
        }

    }
}
public class MultiMoveableObject : MultiInteractable
{
#if UNITY_EDITOR
    protected override Type GetStepType() => typeof(MultiMoveStep);
#endif
    public Queue<ObjectMoveStep> stepQueue = new();

    public IEnumerator DepleteMoveQueue()
    {
        while (stepQueue.Count > 0)
        {
            ObjectMoveStep step = stepQueue.Dequeue();
            yield return StartCoroutine(step.MoveObject());
        }
    }
}
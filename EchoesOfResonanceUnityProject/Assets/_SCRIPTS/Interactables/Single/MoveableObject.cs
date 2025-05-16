using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : BasicInteractable
{
    [SerializeField] protected ObjectMoveStep[] steps;
    [SerializeField] private AnimationCurve interpolationCurve;
    [SerializeField] private AudioEvent startSound;
    private Queue<ObjectMoveStep> stepQueue = new();

    public override void ActivateObject()
    {
        base.ActivateObject();

        foreach (var step in steps)
        {
            step.parent = this;
            stepQueue.Enqueue(step);
        }
        
        CRManager.Begin(DepleteMoveQueue(), $"{gameObject}MoveObject", this);
    }
    public override void ResetObject()
    {
        base.ResetObject();

        for (int i = steps.Length - 1; i >= 0; i--)
        {
            steps[i].parent = this;
            stepQueue.Enqueue(steps[i]);
        }

        CRManager.Begin(DepleteMoveQueue(), $"{gameObject}MoveObject", this);
    }
    public IEnumerator DepleteMoveQueue()
    {
        while (stepQueue.Count > 0)
        {
            ObjectMoveStep step = stepQueue.Dequeue();
            yield return StartCoroutine(step.MoveObject<MoveableObject>(interpolationCurve, startSound));
        }
    }
}


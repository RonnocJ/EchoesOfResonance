using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MultiPhysicsStep : MultiInteractableStep
{
    public PhysicsMoveStep[] steps;
    public override void ActivateObject()
    {
        base.ActivateObject();
        var multiParent = parent as MultiPhysicsObject;

        if (multiParent != null)
        {
            foreach (var step in steps)
            {
                step.parent = multiParent;

                var newStep = step;
                newStep.Forwards = true;

                multiParent.stepQueue.Enqueue(newStep);
            }

            CRManager.Begin(multiParent.DepleteMoveQueue(), $"{multiParent.gameObject}DepleteMoveQueue", multiParent);
        }
    }

    public override void ResetObject()
    {
        base.ResetObject();
        var multiParent = parent as MultiPhysicsObject;

        if (multiParent != null)
        {
            foreach (var step in steps)
            {
                step.parent = multiParent;

                var newStep = step;
                newStep.Forwards = false;

                multiParent.stepQueue.Enqueue(newStep);
            }

            CRManager.Begin(multiParent.DepleteMoveQueue(), $"{multiParent.gameObject}DepleteMoveQueue", multiParent);
        }
    }
}

public class MultiPhysicsObject : MultiInteractable
{
    #if UNITY_EDITOR
        protected override Type GetStepType() => typeof(MultiPhysicsStep);
    #endif
    public bool UsesGravity;
    public bool FreezeOnComplete;
    public Queue<PhysicsMoveStep> stepQueue = new();
    private float _mass, _linearDrag;
    private Rigidbody _rb;
    public override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        _mass = _rb.mass;
        _linearDrag = _rb.linearDamping;

        if(LinkedData != null)
        {
            LinkedData.OnPuzzleCompleted += FreezeObject;
        }
    }
    public IEnumerator DepleteMoveQueue()
    {
        if(UsesGravity) _rb.useGravity = false;

        while (stepQueue.Count > 0)
        {
            PhysicsMoveStep step = stepQueue.Dequeue();
            step.BoostObject<MultiPhysicsObject>(step.Forwards? step.BoostForce : step.ResetForce);
            yield return new WaitForSeconds(step.Time);
        }
        
        _rb.mass = _mass;
        _rb.linearDamping = _linearDrag;

        if(UsesGravity) _rb.useGravity = true;
    }

    public void FreezeObject()
    {
        _rb.isKinematic = true;
    }
}
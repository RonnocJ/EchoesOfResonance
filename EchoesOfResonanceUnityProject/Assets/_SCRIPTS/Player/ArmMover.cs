using System;
using UnityEngine;

public class ArmMover : MonoBehaviour, IInputScript
{
    [Serializable]
    public class Segment
    {
        public Transform Tr;
        public TrData ExtendedTarget;
        public TrData RetractedTarget;
    }

    [SerializeField] private Segment[] _segmentsToMove;
    [SerializeField] private AnimationCurve _movementCurve;
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.Settings, UpdateArmPos);
    }

    public void UpdateArmPos(float setingsInput)
    {
        float truePos = _movementCurve.Evaluate(setingsInput);
        foreach(var seg in _segmentsToMove)
        {
            TrData newPos = new();

            if(setingsInput < 0.05f)
            {
                newPos = seg.ExtendedTarget;
            }
            else if (setingsInput > 0.95f)
            {
                newPos = seg.RetractedTarget;
            }
            else
            {
                newPos = new TrData
                (
                    Vector3.Lerp(seg.ExtendedTarget.position, seg.RetractedTarget.position, truePos),
                    Quaternion.Lerp(Quaternion.Euler(seg.ExtendedTarget.rotation), Quaternion.Euler(seg.RetractedTarget.rotation), truePos),
                    Vector3.Lerp(seg.ExtendedTarget.scale, seg.RetractedTarget.scale, truePos)
                );
            }

            CRManager.root.Restart(newPos.ApplyToOverTime(seg.Tr, 0.25f), $"MoveArmSegment{seg.Tr.name}", this);
        }
    }
}

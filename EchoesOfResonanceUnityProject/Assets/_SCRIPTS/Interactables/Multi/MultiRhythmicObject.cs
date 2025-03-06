using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class RhythmicMoveStep
{
    public NoteValue note;
    public TrData moveStep;
    [HideInInspector]
    public MultiRhythmicObject parent;

    public void MoveObjectWithMusic()
    {
        AudioManager.root.PlaySound((moveStep.EffectedProperties != 0 || !moveStep.IsEqualTo(parent.transform)) ? parent.startSound : AudioEvent.None, parent.gameObject, 1, true);
        CRManager.root.Restart(
                moveStep.ApplyToOverTime(
                    parent.transform,
                    MusicManager.root.currentSong.GetBeatInSeconds() * note.CurrentValue,
                    parent.interpolationCurve
                ),
                $"{parent.gameObject.name}MultiRhythmicMoveStep",
                parent
            );
    }
}
[Serializable]
public class MultiRhythmicStep : MultiInteractableStep
{
    [SerializeField] private RhythmicMoveStep[] steps;
    public override void ActivateObject()
    {
        base.ActivateObject();

        var multiParent = parent as MultiRhythmicObject;
        if (multiParent != null)
        {
            foreach (var step in steps)
            {
                step.parent = multiParent;
                multiParent.stepList.Add(step);
            }
            multiParent.SubscribeToMusic();
        }
    }

    public override void ResetObject()
    {
        base.ResetObject();

        var multiParent = parent as MultiRhythmicObject;
        if (multiParent != null)
        {
            for (int i = steps.Length - 1; i >= 0; i--)
            {
                steps[i].parent = multiParent;
                multiParent.stepList.Add(steps[i]);
            }
            multiParent.SubscribeToMusic();
        }
    }
}
public class MultiRhythmicObject : MultiInteractable
{
#if UNITY_EDITOR
    protected override Type GetStepType() => typeof(MultiRhythmicStep);
#endif
    public AnimationCurve interpolationCurve;
    public AudioEvent startSound;
    public List<RhythmicMoveStep> stepList = new();
    public void SubscribeToMusic()
    {
        foreach(var step in stepList)
        {
            MusicManager.root.currentSong.AddQueuedCallback($"{gameObject.name}RhythmicQueue", step.note.CurrentValue, step.MoveObjectWithMusic);
        }

        stepList.Clear();
    }
}
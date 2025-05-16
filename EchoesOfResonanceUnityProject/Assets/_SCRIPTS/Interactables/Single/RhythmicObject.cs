using System;
using System.Collections.Generic;
using UnityEngine;

public class RhythmicObject : BasicInteractable
{
    public bool looping;
    public AnimationCurve interpolationCurve;
    public AudioEvent startSound;
    public RhythmicMoveStep[] moveSteps;
    private List<RhythmicMoveStep> stepList = new();
    public override void ActivateObject()
    {
        base.ActivateObject();

        foreach (var step in moveSteps)
        {
            step.parent = this;
            stepList.Add(step);
        }
        SubscribeToMusic();
    }

    public override void ResetObject()
    {
        base.ResetObject();
        for (int i = moveSteps.Length - 1; i >= 0; i--)
        {
            stepList.Add(moveSteps[i]);
        }
        SubscribeToMusic();
    }
    public void SubscribeToMusic()
    {
        if (looping)
        {
            foreach (var step in stepList)
            {
                MusicManager.root.currentSong.AddLoopingCallback($"{gameObject}RhythmicLoop", step.note.CurrentValue, () => step.MoveObjectWithMusic<RhythmicObject>(interpolationCurve, startSound));
            }

            stepList.Clear();
        }
        else
        {
            foreach (var step in stepList)
            {
                MusicManager.root.currentSong.AddQueuedCallback($"{gameObject}RhythmicQueue", step.note.CurrentValue, () => step.MoveObjectWithMusic<RhythmicObject>(interpolationCurve, startSound));
            }

            stepList.Clear();
        }
    }
}

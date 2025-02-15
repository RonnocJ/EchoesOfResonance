using System;
using System.Collections.Generic;
using UnityEngine;

public class RhythmicMoveableObject : BasicInteractable, IBeatListener
{
    [Serializable]
    public class RhythmicMoveStep
    {
        public float frequency;
        public float beatDuration;
        public TrData moveStep;
        [HideInInspector]
        public RhythmicMoveableObject parent;

        public void MoveObjectWithMusic()
        {
            CRManager.root.Restart(
                    moveStep.ApplyToOverTime(parent.transform, MusicManager.root.currentSong.GetBeatInSeconds() * frequency, parent.interpolationCurve),
                    $"{parent.gameObject}DoorMoveStep", parent
                );
        }
    }

    public RhythmicMoveStep[] moveSteps;
    public AnimationCurve interpolationCurve;
    private int step = 0;
    private float timeToNextBeat;

    public override void ActivateObject()
    {
        base.ActivateObject();
        SubscribeToMusic();
    }

    public override void ResetObject()
    {
        base.ResetObject();
        UnsubscribeToMusic();
    }

    public void SubscribeToMusic()
    {
        step = 0;
        timeToNextBeat = 0;

        foreach(var step in moveSteps)
        {
            step.parent = this;
        }

        MusicManager.root.currentSong.AddBeatListener(0.25f, StepWithBeat);
    }

    public void UnsubscribeToMusic()
    {
        MusicManager.root.currentSong.RemoveBeatListener(0.25f, StepWithBeat);
    }

    void StepWithBeat(float beat)
    {
        timeToNextBeat += 0.25f;
        if (timeToNextBeat >= moveSteps[step].frequency)
        {
            timeToNextBeat = 0;
            moveSteps[step].MoveObjectWithMusic();

            step = (step + 1) % moveSteps.Length;
        }
    }
}

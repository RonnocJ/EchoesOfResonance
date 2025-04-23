using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
        if (moveStep.EffectedProperties != 0 && !moveStep.IsEqualTo(parent.transform))
        {
            AudioManager.root.PlaySound(parent.startSound, parent.gameObject);
        }

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
    public RhythmicMoveStep[] steps;
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
public class MultiRhythmicObject : MultiInteractable, ISaveData
{
#if UNITY_EDITOR
    protected override Type GetStepType() => typeof(MultiRhythmicStep);
#endif
    public AnimationCurve interpolationCurve;
    public AudioEvent startSound;
    [HideInInspector]
    public List<RhythmicMoveStep> stepList = new();
    public Dictionary<string, object> AddSaveData()
    {
        if ((LinkedData == null && Steps[Steps.Count - 1].activated) || (LinkedData != null && LinkedData.Solved))
        {
            if (Steps[Steps.Count - 1] is MultiRhythmicStep rStep)
            {
                return new Dictionary<string, object>
            {
                {"puzzlePosition", new SaveStruct(rStep.steps[rStep.steps.Length - 1].moveStep)},
            };
            }
        }

        return null;
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        if (savedData.TryGetValue("puzzlePosition", out object solvedPositionRaw))
        {
            string json = JsonConvert.SerializeObject(solvedPositionRaw);
            SaveStruct solvedPosition = JsonConvert.DeserializeObject<SaveStruct>(json);

            TrData solvedPos = solvedPosition.LoadData();
            solvedPos.ApplyTo(transform);
        }
    }
    public void SubscribeToMusic()
    {
        foreach (var step in stepList)
        {
            if (step.moveStep.EffectedProperties == 0 || !step.moveStep.IsEqualTo(transform))
                MusicManager.root.currentSong.AddQueuedCallback($"{gameObject.name}RhythmicQueue", step.note.CurrentValue, step.MoveObjectWithMusic);
        }

        stepList.Clear();
    }
}
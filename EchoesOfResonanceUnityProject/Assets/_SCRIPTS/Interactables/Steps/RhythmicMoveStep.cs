using System;
using UnityEngine;
[Serializable]
public class RhythmicMoveStep
{
    public NoteValue note;
    public TrData moveStep;
    [HideInInspector]
    public object parent;

    public void MoveObjectWithMusic<T>(AnimationCurve interpolationCurve, AudioEvent startSound) where T : MonoBehaviour
    {
        if (parent is T p)
        {
            if (moveStep.EffectedProperties != 0 && !moveStep.IsEqualTo(p.transform))
            {
                AudioManager.root.PlaySound(startSound, p.gameObject);
            }

            CRManager.Restart(
                    moveStep.ApplyToOverTime(
                        p.transform,
                        MusicManager.root.currentSong.GetBeatInSeconds() * note.CurrentValue,
                        interpolationCurve
                    ),
                    $"{p.gameObject.name}MultiRhythmicMoveStep",
                    p
                );
        }
    }
}
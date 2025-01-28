using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[Serializable]
public struct TrData
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public TrData(Vector3 newPos, Quaternion newRot, Vector3 newScale)
    {
        position = newPos;
        rotation = newRot.eulerAngles;
        scale = newScale;
    }
    public void ApplyTo(Transform tr)
    {
        tr.localPosition = position;
        tr.rotation = Quaternion.Euler(rotation);
        tr.localScale = scale;
    }

    public IEnumerator ApplyToOverTime(Transform tr, float duration, bool useGlobalPosition = false)
    {
        Vector3 startPos = useGlobalPosition? tr.position : tr.localPosition;
        Quaternion startRot = tr.rotation;
        Vector3 startScale = tr.localScale;

        Vector3 targetPos = position;
        Quaternion targetRot = Quaternion.Euler(rotation);
        Vector3 targetScale = scale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if(useGlobalPosition)
                tr.position = Vector3.Lerp(startPos, targetPos, t);
            else
                tr.localPosition = Vector3.Lerp(startPos, targetPos, t);
            tr.rotation = Quaternion.Lerp(startRot, targetRot, t);
            tr.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        tr.localPosition = targetPos;
        tr.rotation = targetRot;
        tr.localScale = targetScale;
    }

    public bool IsEqualTo(Transform tr)
    {
        if (tr.localPosition == position && tr.rotation == Quaternion.Euler(rotation) && tr.localScale == scale)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
public enum PuzzleType
{
    Door = 0,
    Torch = 1,
}
public class PuzzleManager : Singleton<PuzzleManager>, IInputScript
{
    private static readonly Dictionary<string, int> NoteOffsets = new Dictionary<string, int>
    {
        {"C", 0}, {"C#", 1},
        {"D", 2}, {"D#", 3},
        {"E", 4},
        {"F", 5}, {"F#", 6},
        {"G", 7}, {"G#", 8},
        {"A", 9}, {"A#", 10},
        {"B", 11}
    };
    public GameObject gemPrefab;

    [SerializeField] private Transform orphanedGemList;
    public PuzzleData currentPuzzle;
    public BasicPuzzle[] currentPuzzleBehaviors;

    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, CheckNote);
    }
    void Start()
    {

        /*var allPuzzles = DataHelper.GetDataOfType<PuzzleData>();

        foreach (var data in allPuzzles)
        {
            if (!BasicPuzzle.instantiatedPuzzles.Contains(data))
            {
                GameObject orphanParent = Instantiate(new GameObject(), orphanedGemList);
                orphanParent.name = $"{data}GemParent";

                for (int i = 0; i < data.solutions.Length; i++)
                {
                    CreateGem(data.solutions[i].correctNote, orphanParent.transform, data.solutions[i].gemTransform);
                }

                data.hasOrphans = true;
            }
        }*/
    }
    public float GetNoteNumber(string note)
    {
        string pitch = note.Substring(0, note.Length - 1);
        if (!int.TryParse(note.Substring(note.Length - 1), out int octave))
            throw new ArgumentException($"Invalid note format: {note}");

        if (!NoteOffsets.TryGetValue(pitch, out int pitchOffset))
            throw new ArgumentException($"Invalid pitch: {pitch}");

        return (octave - 1) * 12 + pitchOffset + 1;
    }
    public string GetNoteName(float noteNumber)
    {
        int noteIndex = (int)((noteNumber - 1) % 12);
        int octave = (int)((noteNumber - 1) / 12) + 1;

        string pitch = NoteOffsets.FirstOrDefault(x => x.Value == noteIndex).Key;
        if (pitch == null)
            throw new ArgumentException($"Invalid note number: {noteNumber}");

        return $"{pitch}{octave}";
    }
    [AllowedStates(GameState.InPuzzle)]
    public void CheckNote(float noteInput)
    {
        if (currentPuzzle != null || currentPuzzle.solved < currentPuzzle.solutions.Length)
        {
            if (noteInput == GetNoteNumber(currentPuzzle.solutions[currentPuzzle.solved].correctNote))
            {
                if (!currentPuzzle.hasOrphans)
                {
                    foreach (var b in currentPuzzleBehaviors)
                        b.GetGemChild(currentPuzzle.solved)?.LightOn();
                }
                else
                {
                    currentPuzzleBehaviors[0].orphanParent.GetChild(currentPuzzle.solved).GetComponent<Gem>().LightOn();
                }

                currentPuzzle.solved++;
            }
            else
            {
                ResetPuzzle();
            }
        }
    }

    public void ResetPuzzle()
    {
        for (int i = currentPuzzle.solved; i > -1; i--)
        {
            if (!currentPuzzle.hasOrphans)
            {
                foreach (var b in currentPuzzleBehaviors)
                {
                    CRManager.root.Begin(b.GetComponent<TorchManager>()?.ExtinguishTorch(false, 0f), $"Extinguish{b.gameObject.name}", this);
                    b.GetGemChild(i)?.LightOff();
                }
            }
            else
            {
                currentPuzzleBehaviors[0].orphanParent.GetChild(i).GetComponent<Gem>().LightOff();
            }

        }

        currentPuzzle.solved = 0;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[Serializable]
public struct TrData
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public void ApplyTo(Transform tr)
    {
        tr.localPosition = position;
        tr.rotation = Quaternion.Euler(rotation);
        tr.localScale = scale;
    }
}
public enum PuzzleType
{
    Door = 0,
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
    [SerializeField] private Mesh[] gemMeshes;
    [SerializeField] private Color[] gemColors;
    public PuzzleData currentPuzzle;
    public BasicPuzzle[] currentPuzzleBehaviors;

    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, CheckNote);
    }
    public void CreateGem(string newNoteString, Transform parent, TrData trData)
    {
        float newNoteFloat = GetNoteNumber(newNoteString);
        GameObject newGemObj = Instantiate(gemPrefab, parent);
        trData.ApplyTo(newGemObj.transform);
        newGemObj.transform.GetChild(0).GetComponent<MeshFilter>().mesh = gemMeshes[((int)newNoteFloat - 1) % 5];
        newGemObj.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", gemColors[Mathf.FloorToInt((newNoteFloat - 1) / 5f)]);
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
    public void CheckNote(float noteInput)
    {
        if (GameManager.root.currentState == GameState.InPuzzle && (currentPuzzle != null || currentPuzzle.solved == currentPuzzle.solutions.Length))
        {
            if (noteInput - ConfigureKeyboard.middleKey + 13 == GetNoteNumber(currentPuzzle.solutions[currentPuzzle.solved].correctNote))
            {
                foreach(var b in currentPuzzleBehaviors)
                b.GetGemChild(currentPuzzle.solved)?.LightOn();
                currentPuzzle.solved++;
            }
            else
            {
                for (int i = currentPuzzle.solved; i > -1; i--)
                {
                    foreach(var b in currentPuzzleBehaviors)
                    b.GetGemChild(i)?.LightOff();
                }
                currentPuzzle.solved = 0;
            }

            if (currentPuzzle.solved == currentPuzzle.solutions.Length)
            {
foreach(var b in currentPuzzleBehaviors)
                b.FinishedPuzzle();
            }
        }
    }
}
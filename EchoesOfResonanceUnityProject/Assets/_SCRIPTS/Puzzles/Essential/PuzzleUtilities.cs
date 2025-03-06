using System;
using System.Collections.Generic;
using System.Linq;

public enum PuzzleType
{
    Door = 0,
    Torch = 1,
    FakePlate = 2,
}
public class PuzzleUtilities : Singleton<PuzzleUtilities>
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
}
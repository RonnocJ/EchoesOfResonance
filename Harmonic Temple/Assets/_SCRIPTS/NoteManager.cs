using System.Collections.Generic;
using Minis;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoteManager : MonoBehaviour
{
    public List<GameObject> heldNotes = new List<GameObject>();
    public DoorManager doorManager;
    public AK.Wwise.Event playTone, stopTone;
    public AK.Wwise.RTPC tonePitch;
    private MidiDevice midiController;
    void Start()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change != InputDeviceChange.Added) return;

            midiController = device as MidiDevice;
            if (midiController == null) return;

            midiController.onWillNoteOn += (note, velocity) =>
            {
                string newNote = note.shortDisplayName.Remove(note.shortDisplayName.Length - 1);
                doorManager.CheckNote(newNote);
                playTone.Post(heldNotes[GetNote(newNote)]);
                tonePitch.SetValue(heldNotes[GetNote(newNote)], GetNote(newNote));
            };
            midiController.onWillNoteOff += (note) =>
            {
                string oldNote = note.shortDisplayName.Remove(note.shortDisplayName.Length - 1);
                Debug.Log($"Released note {note.shortDisplayName}");
                stopTone.Post(heldNotes[GetNote(oldNote)]);
            };
        };
    }

    private int GetNote(string noteInput)
    {
        switch (noteInput)
        {
            case "A":
                return 1;
            case "A#":
                return 2;
            case "B":
                return 3;
            case "C":
                return 4;
            case "C#":
                return 5;
            case "D":
                return 6;
            case "D#":
                return 7;
            case "E":
                return 8;
            case "F":
                return 9;
            case "F#":
                return 10;
            case "G":
                return 11;
            case "G#":
                return 12;
            default:
                return 0;
        }
    }
}
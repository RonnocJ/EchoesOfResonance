using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private List<string> activeNotes = new List<string>();
    void Start()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change != InputDeviceChange.Added) return;

            midiController = device as MidiDevice;
            if (midiController == null) return;

            midiController.onWillNoteOn += NoteOn;
            midiController.onWillNoteOff += note => NoteOff(note.shortDisplayName);
        };
    }
    private void NoteOn(MidiNoteControl note, float velocity)
    {
        activeNotes.Add(note.shortDisplayName);
        string newNote = note.shortDisplayName.Remove(note.shortDisplayName.Length - 1);
        doorManager.CheckNote(newNote);
        playTone.Post(heldNotes[GetNote(newNote) - 1]);
        tonePitch.SetValue(heldNotes[GetNote(newNote) - 1], GetNote(newNote));
    }
    private void NoteOff(string note)
    {
        if (activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
            string oldNote = note.Remove(note.Length - 1);
            doorManager.RemoveNote(oldNote);
            Debug.Log($"Released note {note}");
            stopTone.Post(heldNotes[GetNote(oldNote) - 1]);
        }
    }
    public void AllNotesOff()
    {
        foreach(string note in activeNotes)
        {
            NoteOff(note);
        }
    }

    void OnDestroy()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change != InputDeviceChange.Removed) return;

            midiController = device as MidiDevice;
            if (midiController == null) return;

            midiController.onWillNoteOn -= NoteOn;
            midiController.onWillNoteOff -= note => NoteOff(note.shortDisplayName);
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Minis;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoteManager : Singleton<NoteManager>, IInputScript
{
    public DoorManager doorManager;
    public AK.Wwise.Event playTone, stopTone;
    public AK.Wwise.RTPC tonePitch;
    private List<GameObject> heldNotes = new List<GameObject>();
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, NoteOn);
        InputManager.root.AddListener<float>(ActionTypes.KeyUp, NoteOff);
    }
    void Start()
    {
        var noteTr = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform tr in noteTr)
        {
            heldNotes.Add(tr.gameObject);
        }
    }
    private void NoteOn(float noteNumber)
    {
        if (GameManager.root.currentState != GameState.Config)
        {
            float trueNoteNumber = noteNumber - ConfigureKeyboard.middleKey + 13;
            playTone.Post(heldNotes[(int)trueNoteNumber]);
            tonePitch.SetValue(heldNotes[(int)trueNoteNumber], trueNoteNumber);
        }
    }
    private void NoteOff(float noteNumber)
    {
        if (GameManager.root.currentState != GameState.Config)
        {
            float trueNoteNumber = noteNumber - ConfigureKeyboard.middleKey + 13;
            stopTone.Post(heldNotes[(int)trueNoteNumber]);
        }
    }
    public void AllNotesOff()
    {
        foreach (GameObject obj in heldNotes)
        {
            stopTone.Post(obj);
        }
    }
}
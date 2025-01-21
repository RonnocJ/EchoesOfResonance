using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Minis;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoteManager : Singleton<NoteManager>, IInputScript
{
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
    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    private void NoteOn(float noteNumber)
    {
        playTone.Post(heldNotes[(int)noteNumber]);
            tonePitch.SetValue(heldNotes[(int)noteNumber], noteNumber);
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    private void NoteOff(float noteNumber)
    {
        stopTone.Post(heldNotes[(int)noteNumber]);
    }
    public void AllNotesOff()
    {
        foreach (GameObject obj in heldNotes)
        {
            stopTone.Post(obj);
        }
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BrNotes : MonoBehaviour, IInputScript
{
    public void AddInputs()
    {
        InputManager.root.AddVelocityListener<(int, int)>(ActionTypes.KeyDown, PlayNote);
        InputManager.root.AddListener<float>(ActionTypes.KeyUp, RemoveNote);
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming, GameState.Synced)]
    void PlayNote(int newNote, int noteVelocity)
    {
        AudioManager.root.PlaySound(AudioEvent.playBroadcasterFX, gameObject, 1);
        AudioManager.root.PlaySound(AudioEvent.playBroadcasterNote, gameObject, newNote);
        AudioManager.root.SetRTPC(AudioRTPC.flute_Pitch, newNote, false, AudioEvent.playBroadcasterNote, gameObject, newNote);
        AudioManager.root.SetRTPC(AudioRTPC.flute_Velocity, noteVelocity, false, AudioEvent.playBroadcasterNote, gameObject, newNote);

        BrBattery.root.notesHeld++;

        BrDisplay.root.SetPlayingText(PzUtil.GetNoteName(newNote), true);         
    }

    [AllowAllAboveState(GameState.InPuzzle), DissallowedStates(GameState.Intro)]
    public void RemoveNote(float oldNote)
    {
        if (AudioManager.root.StopSound(AudioEvent.playBroadcasterNote, gameObject, (int)oldNote))
            BrBattery.root.notesHeld--;

        BrDisplay.root.SetPlayingText(PzUtil.GetNoteName(oldNote), false);
    }
}
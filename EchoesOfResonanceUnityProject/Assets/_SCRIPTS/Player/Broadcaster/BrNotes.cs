using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BrNotes : MonoBehaviour, IInputScript
{
    public int sineResolution;
    [SerializeField] private float notePlayDrainAmount, noteSustainDrainAmount;
    [SerializeField] private LineRenderer sineWave;
    private float frequency, speed, lastNotePlayed;
    private Vector3[] sinePoints;
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, PlayNote);
        InputManager.root.AddListener<float>(ActionTypes.KeyUp, RemoveNote);
    }
    void Start()
    {
        lastNotePlayed = 13;

        sineWave.positionCount = sineResolution;
        sinePoints = new Vector3[sineResolution];

        for (int i = 0; i < sineResolution; i++)
        {
            sinePoints[i].x = (i * 1f / sineResolution) - 0.5f;
        }
        sineWave.SetPositions(sinePoints);

        BrBattery.root.OnBatteryEmpty += () => AllNotesOff();
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    void PlayNote(float newNote)
    {
        BrBattery.root.batteryLevel -= notePlayDrainAmount;

        if (BrBattery.root.notesHeld == 0)
        {
            AudioManager.root.PlaySound(AudioEvent.playBroadcasterFX, gameObject);
        }

        AudioManager.root.PlaySound(AudioEvent.playBroadcasterNote, gameObject, newNote);
        AudioManager.root.SetRTPC(AudioRTPC.flute_Pitch, newNote, false, AudioEvent.playBroadcasterNote, gameObject, newNote);

        BrBattery.root.notesHeld++;
        lastNotePlayed = newNote;
        frequency = 2 + (lastNotePlayed * 0.32f);
        speed = 1 + (lastNotePlayed * 0.36f);

        BrBattery.root.noteInfoText.text = PuzzleUtilities.root.GetNoteName(lastNotePlayed);

        CRManager.root.Restart(BrBattery.root.DrainBatteryRoutine(noteSustainDrainAmount * BrBattery.root.notesHeld), "DrainBatteryNote", this);
    }

    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    public void RemoveNote(float oldNote)
    {
        if (AudioManager.root.StopSound(AudioEvent.playBroadcasterNote, gameObject, oldNote))
            BrBattery.root.notesHeld--;

        if (BrBattery.root.notesHeld == 0)
        {
            BrBattery.root.noteInfoText.text = "";
            CRManager.root.Stop("DrainBatteryNote", this);
        }
    }
    public void AllNotesOff()
    {
        for (int i = 1; i < 26; i++)
        {
            AudioManager.root.StopSound(AudioEvent.playBroadcasterNote, gameObject, i);
        }
    }
    void Update()
    {
        if (BrBattery.root.notesHeld > 0)
        {
            for (int i = 0; i < sineResolution; i++)
            {
                sinePoints[i].y = Mathf.Lerp(sinePoints[i].y, 0.1f * Mathf.Sin((frequency * sinePoints[i].x * 2) + (Time.timeSinceLevelLoad * speed) + frequency / 2), Time.deltaTime * 10);
            }
        }
        else
        {
            for (int i = 0; i < sineResolution; i++)
            {
                sinePoints[i].y = Mathf.Lerp(sinePoints[i].y, 0, Time.deltaTime * 10);
            }
        }

        sineWave.SetPositions(sinePoints);
    }
}
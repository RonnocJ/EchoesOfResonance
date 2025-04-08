using System;
using System.Collections.Generic;
using UnityEngine;

public class BrSettings : Singleton<BrSettings>, IInputScript, ISaveData
{
    [HideInInspector]
    public float middleKey, tempMiddleKey;
    private GameState oldState;
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.Settings, OpenSettings);
    }
    public Dictionary<string, object> AddSaveData()
    {
        return new Dictionary<string, object>
        {
            {"middleKey", middleKey},
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        this.middleKey = -1f;

        if (savedData.TryGetValue("middleKey", out object middleKey))
        {
            this.middleKey = Convert.ToSingle(middleKey);
        }
    }
    [AllowedStates(GameState.Settings, GameState.InPuzzle, GameState.Roaming, GameState.Shutdown)]
    public void OpenSettings(float newPauseAmount)
    {
        if (newPauseAmount < 0.5f && oldState != GameState.Settings)
        {
            GameManager.root.currentState = oldState;

            AudioManager.root.PlaySound(AudioEvent.resumeAll);
        }
        else if (newPauseAmount > 0.5f && !(GameManager.root.currentState is GameState.Settings or GameState.Config or GameState.Cutscene))
        {
            oldState = GameManager.root.currentState;
            GameManager.root.currentState = GameState.Settings;

            AudioManager.root.PlaySound(AudioEvent.pauseAll);

            PlayerManager.root.lookInput = 0;
            PlayerManager.root.moveInput = 0;
        }
    }
}

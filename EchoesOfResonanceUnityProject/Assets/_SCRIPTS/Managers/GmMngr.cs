using System;
using System.Collections.Generic;
using UnityEngine;
public enum GameState
{
    Config = 0,
    Cutscene = 1,
    Settings = 2,
    InPuzzle = 3,
    Intro = 4,
    Shutdown = 5,
    Roaming = 6,
    Synced = 7
}
public class GameManager : Singleton<GameManager>, ISaveData
{
    public GameState State
    {
        get => _currentState;
        set
        {
            if (value is GameState.Cutscene or GameState.Settings)
            {
                _lastState = _currentState;
                _currentState = value;
            }
            else
            {
                _currentState = value;
            }
        }
    }
    public PuzzleData currentPuzzle;
    public PuzzlePlate currentPlate;
    [SerializeField] private GameState _currentState;
    private GameState _lastState;
    public Dictionary<string, object> AddSaveData()
    {
        switch (State)
        {
            case GameState.Config or GameState.Cutscene:
                return new()
                {
                    {"savedState", _lastState}
                };
            case GameState.Settings:
                if (_lastState != GameState.InPuzzle)
                {
                    return new()
                    {
                        {"savedState", _lastState}
                    };
                }
                else
                {
                    return new()
                    {
                        {"savedState", GameState.Roaming}
                    };
                }
            case GameState.InPuzzle or GameState.Shutdown or GameState.Synced:
                return new()
                {
                    {"savedState", GameState.Roaming}
                };
            default:
                return new()
                {
                    {"savedState", State}
                };

        }
    }

    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        if (savedData.TryGetValue("savedState", out object oldSavedState))
        {
            Enum.TryParse(Convert.ToString(oldSavedState), out GameState oldSavedStateEnum);
            _lastState = oldSavedStateEnum;
            MusicManager.root.GameStateToSet = oldSavedStateEnum;
        }
        else
        {
            MusicManager.root.GameStateToSet = GameState.Intro;
        }
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;
public enum GameState
{
    Config,
    Intro,
    Cutscene,
    Settings,
    InPuzzle,
    Roaming,
    Shutdown,
    Final
}
public class GameManager : Singleton<GameManager>, ISaveData
{
    public GameState currentState;
    public PuzzleData currentPuzzle;
    public PuzzlePlate currentPlate;
    public Dictionary<string, object> AddSaveData()
    {
        if (currentState != GameState.InPuzzle)
        {
            return new()
            {
                {"savedState", currentState}
            };
        }
        else
        {
            return new()
            {
                {"savedState", GameState.Roaming}
            };
        }

    }

    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        if (savedData.TryGetValue("savedState", out object oldSavedState))
        {
            Enum.TryParse(Convert.ToString(oldSavedState), out GameState oldSavedStateEnum);
            MusicManager.root.GameStateToSet = oldSavedStateEnum;
        }
        else
        {
            MusicManager.root.GameStateToSet = GameState.Intro;
        }
    }
}


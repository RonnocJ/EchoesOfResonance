using System;
using UnityEngine;
public enum GameState
{
    Config,
    Title,
    Settings,
    InPuzzle,
    Roaming,
    Shutdown,
    Final
}
public class GameManager : Singleton<GameManager>
{
    public GameState currentState;
    public PuzzleData currentPuzzle;
    public PuzzlePlate currentPlate;

    void Start()
    {
        if(DH.Get<TestOverrides>().skipIntro && DH.Get<TestOverrides>().ignoreMidi)
        {
            currentState = GameState.Roaming;
        }
    }
}


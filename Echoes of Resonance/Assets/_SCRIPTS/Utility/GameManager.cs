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
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AllowedStatesAttribute : Attribute
{
    public GameState[] States { get; }

    public AllowedStatesAttribute(params GameState[] states)
    {
        States = states;
    }
}

using UnityEngine;
public enum GameState
{
    Config,
    InPuzzle,
    Roaming,
    Shutdown
}
public class GameManager : Singleton<GameManager>
{
    public GameState currentState;
}
using UnityEngine;
public enum GameState
{
    Config,
    Title,
    InPuzzle,
    Roaming,
    Shutdown
}
public class GameManager : Singleton<GameManager>
{
    public GameState currentState;
}
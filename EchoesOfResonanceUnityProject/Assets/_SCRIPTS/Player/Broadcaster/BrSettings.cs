using UnityEngine;

public class BrSettings : MonoBehaviour, IInputScript
{
    private GameState oldState;
    void Start()
    {
        if(DH.Get<TestOverrides>().skipIntro) oldState = GameState.Roaming;
        else oldState = GameState.Title;
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.Settings, OpenSettings);
    }

    [AllowedStates(GameState.Settings, GameState.InPuzzle, GameState.Roaming, GameState.Shutdown)]
    public void OpenSettings(float newPauseAmount)
    {   
        if (newPauseAmount < 0.5f && oldState != GameState.Settings)
        {
            GameManager.root.currentState = oldState;
        }
        else if (newPauseAmount > 0.5f && !(GameManager.root.currentState is GameState.Settings or GameState.Config or GameState.Title))
        {
            oldState = GameManager.root.currentState;
            GameManager.root.currentState = GameState.Settings;
        }

        GameManager.root.currentState = GameState.Roaming;
    }
}

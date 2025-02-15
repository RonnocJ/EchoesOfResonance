using UnityEngine;

public class BrSettings : MonoBehaviour, IInputScript
{
    [SerializeField] private TrData handTr, faceTr;
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
        TrData newPos = new TrData(
            Vector3.Lerp(handTr.position, faceTr.position, Mathf.Pow(newPauseAmount, 2)),
            Quaternion.Lerp(Quaternion.Euler(handTr.rotation), Quaternion.Euler(faceTr.rotation), Mathf.Pow(newPauseAmount, 2)),
            Vector3.Lerp(handTr.scale, faceTr.scale, Mathf.Pow(newPauseAmount, 2))
            );

        CRManager.root.Restart(newPos.ApplyToOverTime(transform, 0.25f), "MoveBroadcaster", this);

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

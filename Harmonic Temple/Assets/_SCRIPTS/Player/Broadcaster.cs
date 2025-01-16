using System.Collections;
using UnityEngine;

public class Broadcaster : MonoBehaviour, IInputScript
{
    [SerializeField] private LineRenderer sineWave;
    public int resolution;
    private float frequency, speed, lastNotePlayed;

    void Awake()
    {
        sineWave.positionCount = resolution;
    }

    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, SetLastNote);
    }
    void SetLastNote(float newNote)
    {
        lastNotePlayed = newNote - ConfigureKeyboard.middleKey + 13;
        frequency = 2 + (lastNotePlayed * 0.32f);
        speed = 1 + (lastNotePlayed * 0.36f);

    }
    void Update()
    {
        if (GameManager.root.currentState is GameState.InPuzzle or GameState.Roaming)
        {
            if (sineWave.positionCount != resolution)
            {
                sineWave.positionCount = resolution;
            }

            for (int i = 0; i < resolution; i++)
            {
                float x = (i * 2f / resolution) - 1f;
                float y = 0.1f * Mathf.Sin((frequency * x) + (Time.timeSinceLevelLoad * speed));
                sineWave.SetPosition(i, new Vector3(x / 2f, y, 0));
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicPuzzle : MonoBehaviour
{
    public PuzzleData linkedData;

    public virtual void FinishedPuzzle()
    {
        GameManager.root.currentState = GameState.Roaming;
    }
}
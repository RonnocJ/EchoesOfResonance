using System.Collections.Generic;
using UnityEngine;

public abstract class BasicPuzzle : MonoBehaviour
{
    public static HashSet<PuzzleData> instantiatedPuzzles = new HashSet<PuzzleData>();

    [SerializeField]
    protected PuzzleData attachedData;
    public bool noGems;
    [HideInInspector]
    public Transform orphanParent;

    public virtual void Awake()
    {
        attachedData.solved = 0;
    }

    public Gem GetGemChild(int gemIndex)
    {
        if(noGems)
            return null;

        if (transform.GetChild(gemIndex)?.GetComponent<Gem>() != null)
            return transform.GetChild(gemIndex).GetComponent<Gem>();

        return null;
    }

    public virtual void FinishedPuzzle()
    {
        GameManager.root.currentState = GameState.Roaming;
    }
}
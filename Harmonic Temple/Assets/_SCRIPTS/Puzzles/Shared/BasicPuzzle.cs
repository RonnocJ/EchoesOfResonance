using System.Collections.Generic;
using UnityEngine;

public abstract class BasicPuzzle : MonoBehaviour
{
    private static HashSet<PuzzleData> instantiatedPuzzles = new HashSet<PuzzleData>();

    [SerializeField]
    protected PuzzleData attachedData;

    void Awake()
    {
        if (attachedData != null && !instantiatedPuzzles.Contains(attachedData))
        {
            instantiatedPuzzles.Add(attachedData);
                    attachedData.solved = 0;
        for (int i = 0; i < attachedData.solutions.Length; i++)
        {
            PuzzleManager.root.CreateGem(attachedData.solutions[i].correctNote, transform, attachedData.solutions[i].gemTransform);
        }
        }
    }

    public Gem GetGemChild(int gemIndex)
    {
        if (transform.GetChild(gemIndex).GetComponent<Gem>() != null)
            return transform.GetChild(gemIndex).GetComponent<Gem>();
        else
            return null;
    }

    public virtual void FinishedPuzzle()
    {
        GameManager.root.currentState = GameState.Roaming;
    }
}
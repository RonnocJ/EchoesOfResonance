using UnityEngine;

public class SinglePuzzleStand : PuzzleStand
{
    [SerializeField] private PuzzleData linkedData;
    public void Awake()
    {
        linkedData.solved = 0;
        linkedData.Active = false;
        activeData = linkedData;
    }

    public override void EjectPlayer()
    {
        base.EjectPlayer();
    }
}
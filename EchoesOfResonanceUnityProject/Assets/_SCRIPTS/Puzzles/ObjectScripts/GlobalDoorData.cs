using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/Global/GlobalDoorData", order = 2)]
public class GlobalDoorData : GlobalData
{
    public float doorMoveSpeed;
    public GameObject doorOpenParticle;
    public AK.Wwise.Event doorOpening;

}

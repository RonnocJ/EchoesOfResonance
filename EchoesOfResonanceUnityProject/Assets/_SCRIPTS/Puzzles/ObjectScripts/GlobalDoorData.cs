using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/Global/GlobalDoorData", order = 1)]
public class GlobalDoorData : ScriptableObject
{
    public float doorMoveSpeed;
    public GameObject doorOpenParticle;
    public AK.Wwise.Event doorOpening;
}

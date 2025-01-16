using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/GlobalDoorData", order = 1)]
public class GlobalDoorData : ScriptableObject
{
    public float doorMoveSpeed;
    public GameObject doorOpenParticle;
    public AK.Wwise.Event doorStartOpening, doorFinishOpening;
}

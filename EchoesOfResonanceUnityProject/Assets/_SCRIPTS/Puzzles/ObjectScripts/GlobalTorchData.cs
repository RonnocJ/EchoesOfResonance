using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/Global/GlobalTorchData", order = 2)]
public class GlobalTorchData : ScriptableObject
{
    public AK.Wwise.Event torchLightUp, torchExtinguish;
}
